using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using BasicWebNovelAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing novel chapters
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly BasicWebNovelContext _context;
        private readonly IChapterAccessService _chapterAccessService;

        public ChapterController(
            IChapterRepository chapterRepository, 
            IWebHostEnvironment environment, 
            BasicWebNovelContext context,
            IChapterAccessService chapterAccessService)
        {
            _chapterRepository = chapterRepository;
            _environment = environment;
            _context = context;
            _chapterAccessService = chapterAccessService;
        }

        /// <summary>
        /// Uploads a PDF file to use as chapter content
        /// </summary>
        /// <param name="userId">User ID of the uploader</param>
        /// <param name="novelId">Novel ID that the chapter belongs to</param>
        /// <param name="chapterId">Chapter ID (0 if creating a new chapter)</param>
        /// <param name="file">The PDF file to upload</param>
        /// <returns>The path to the uploaded PDF file</returns>
        /// <response code="200">File uploaded successfully</response>
        /// <response code="400">If file is invalid or upload fails</response>
        [HttpPost("upload-pdf/{userId}/{novelId}/{chapterId?}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadPdf(int userId, int novelId, int chapterId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file was provided.");

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only PDF files are allowed.");

                // Upload to local storage
                string pdfPath = await _chapterRepository.UploadPdfAsync(file, userId, novelId);
                
                return Ok(new { 
                    pdfPath = pdfPath,     // Relative path for server-side operations
                    pdfUrl = $"/{pdfPath}"  // URL for client-side access
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to upload file: {ex.Message}");
            }
        }

        /// <summary>
        /// Replaces a chapter's PDF file
        /// </summary>
        /// <param name="userId">User ID of the uploader</param>
        /// <param name="novelId">Novel ID that the chapter belongs to</param>
        /// <param name="chapterId">Chapter ID to update</param>
        /// <param name="file">The new PDF file to upload</param>
        /// <returns>Result with updated chapter information</returns>
        /// <response code="200">File replaced successfully</response>
        /// <response code="400">If file is invalid or upload fails</response>
        /// <response code="404">If novel or chapter not found</response>
        [HttpPost("replace-pdf/{userId}/{novelId}/{chapterId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReplacePdf(int userId, int novelId, int chapterId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file was provided.");

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only PDF files are allowed.");
                
                // Get existing chapter to check for PDF
                var chapter = await _context.Chapters
                    .FirstOrDefaultAsync(c => c.Id == chapterId && c.NovelId == novelId);
                
                if (chapter == null)
                    return NotFound("Chapter not found.");
                
                // Upload new PDF to local storage
                string pdfPath = await _chapterRepository.UploadPdfAsync(file, userId, novelId);
                
                // Create update DTO with the new PDF path
                var updateDto = new UpdateChapterDto
                {
                    Title = chapter.Title,
                    Content = chapter.Content,
                    ChapterNumber = chapter.ChapterNumber,
                    PdfPath = pdfPath,
                    UsePdfContent = true // Set to true since we're uploading a PDF
                };
                
                // Update the chapter - this will handle deleting the old PDF
                bool updated = await _chapterRepository.UpdateChapterAsync(novelId, userId, chapterId, updateDto);
                
                if (!updated)
                    return BadRequest("Failed to update chapter with new PDF.");
                
                return Ok(new { 
                    pdfPath = pdfPath,     // Relative path for server-side operations
                    pdfUrl = $"/{pdfPath}",  // URL for client-side access
                    updated = true
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to replace PDF file: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new chapter for a novel
        /// </summary>
        /// <param name="userId">The unique identifier of the user creating the chapter</param>
        /// <param name="novelId">The unique identifier of the novel to add the chapter to</param>
        /// <param name="createChapterDto">Object containing the new chapter details</param>
        /// <returns>The newly created chapter with its assigned ID</returns>
        /// <response code="201">Returns the created chapter</response>
        /// <response code="400">If the chapter data is invalid</response>
        /// <response code="404">If the novel is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" role.
        /// Users can only add chapters to novels they have created, unless they are administrators.
        /// 
        /// Sample request:
        ///
        ///     POST /api/Chapter/create-chapter/1/2
        ///     {
        ///         "title": "Chapter One: The Beginning",
        ///         "content": "Once upon a time in a land far away...",
        ///         "chapterNumber": 1
        ///     }
        /// </remarks>
        [HttpPost("create-chapter/{userId}/{novelId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapter(int userId, int novelId, [FromBody] CreateChapterDto createChapterDto)
        {
            try 
            {
                var chapter = await _chapterRepository.AddChapterToNovelAsync(novelId, userId, createChapterDto);

                return CreatedAtAction(nameof(CreateChapter), new { userId = userId, novelId = novelId, chapterId = chapter.Id }, chapter);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing chapter
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="chapterId">The unique identifier of the chapter to update</param>
        /// <param name="userId">The unique identifier of the user making the update</param>
        /// <param name="updateChapterDto">Object containing the updated chapter information</param>
        /// <returns>Confirmation of successful update</returns>
        /// <response code="200">Chapter updated successfully</response>
        /// <response code="404">If the novel or chapter is not found</response>
        /// <response code="400">If the update data is invalid</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" role.
        /// Users can only update chapters for novels they have created, unless they are administrators.
        /// 
        /// Sample request:
        ///
        ///     PUT /api/Chapter/update-chapter/2/3/1
        ///     {
        ///         "title": "Updated Chapter Title",
        ///         "content": "Updated chapter content goes here...",
        ///         "isPublished": true
        ///     }
        /// </remarks>
        [HttpPut("update-chapter/{novelId}/{chapterId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateChapter(int novelId, int chapterId, int userId, [FromBody] UpdateChapterDto updateChapterDto)
        {
            try 
            {
                var isUpdated = await _chapterRepository.UpdateChapterAsync(novelId, userId, chapterId, updateChapterDto);
                if (!isUpdated)
                {
                    return NotFound("Novel or chapter not found.");
                }
                return Ok("Chapter updated successfully.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a chapter
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="chapterId">The unique identifier of the chapter to delete</param>
        /// <param name="userId">The unique identifier of the user requesting deletion</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Chapter deleted successfully</response>
        /// <response code="404">If the novel or chapter is not found</response>
        /// <response code="400">If there was an error deleting the chapter</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "Admin" or "User" role.
        /// Users can only delete chapters for novels they have created, unless they are administrators.
        /// </remarks>
        [HttpDelete("delete-chapter/{novelId}/{chapterId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteChapter(int novelId, int chapterId, int userId)
        {
            try 
            {
                var isDeleted = await _chapterRepository.DeleteChapterAsync(novelId, userId, chapterId);

                if (!isDeleted)
                {
                    return NotFound("Novel or chapter not found.");
                }

                return Ok("Chapter deleted successfully.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all chapters for a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <returns>List of all chapters for the specified novel</returns>
        /// <response code="200">Returns the list of chapters</response>
        /// <response code="404">If the novel is not found</response>
        /// <response code="400">If there was an error retrieving the chapters</response>
        /// <remarks>
        /// Chapters are returned in order of their chapter numbers.
        /// </remarks>
        [HttpGet("novel-all-chapters/{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllChapters(int novelId)
        {
            try
            {
                var chapters = await _chapterRepository.GetAllChaptersAsync(novelId);
                
                // Get the current user ID if authenticated
                int userId = 0;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    int.TryParse(userIdClaim, out userId);
                }

                // Evaluate access status for each chapter
                bool isAuthorOrAdmin = false;
                if (userId > 0)
                {
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    var novel = await _context.Novels.FindAsync(novelId);
                    if (user != null && novel != null)
                    {
                        if (user.Role?.RoleName == "Admin" || novel.UserId == userId)
                        {
                            isAuthorOrAdmin = true;
                        }
                    }
                }

                var pricing = await _context.ChapterPricings.FirstOrDefaultAsync(p => p.NovelId == novelId);
                int freeChaptersCount = pricing?.FreeChaptersCount ?? 10;
                int coinPrice = pricing?.CoinPricePerChapter ?? 1;
                bool scheduleEnabled = pricing?.UnlockScheduleEnabled ?? false;
                int intervalDays = pricing?.UnlockIntervalDays ?? 7;
                DateTime? startDate = pricing?.ScheduleStartDate;

                var unlocks = new HashSet<int>();
                if (userId > 0)
                {
                    unlocks = (await _context.UserChapterUnlocks
                        .Where(u => u.UserId == userId && u.Chapter.NovelId == novelId)
                        .Select(u => u.ChapterId)
                        .ToListAsync())
                        .ToHashSet();
                }

                var now = DateTime.UtcNow;

                foreach (var chapter in chapters)
                {
                    bool isFree = chapter.ChapterNumber <= freeChaptersCount;
                    bool isScheduleUnlocked = false;

                    if (!isFree && scheduleEnabled && startDate.HasValue && now >= startDate.Value)
                    {
                        var daysSinceStart = (now - startDate.Value).TotalDays;
                        var opened = (int)Math.Floor(daysSinceStart / intervalDays);
                        if (chapter.ChapterNumber <= freeChaptersCount + opened)
                        {
                            isScheduleUnlocked = true;
                        }
                    }

                    bool isPurchased = unlocks.Contains(chapter.Id);
                    
                    chapter.IsFree = isFree;
                    chapter.CoinPrice = isFree ? 0 : coinPrice;
                    chapter.IsAccessible = isAuthorOrAdmin || isFree || isScheduleUnlocked || isPurchased;
                }

                return Ok(chapters);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a specific chapter by its number
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="chapterNumber">The sequential number of the chapter</param>
        /// <param name="userId">Optional user ID for tracking reading progress (0 means anonymous)</param>
        /// <returns>Chapter details and content</returns>
        /// <response code="200">Returns the chapter details</response>
        /// <response code="404">If the novel or chapter is not found</response>
        /// <response code="400">If there was an error retrieving the chapter</response>
        /// <remarks>
        /// If a valid userId is provided, this endpoint will update the user's reading progress.
        /// </remarks>
        [HttpGet("get-chapter/{novelId}/{chapterNumber}/{userId?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChapterByNumber(int novelId, int chapterNumber, int userId = 0)
        {
            try
            {
                var chapter = await _chapterRepository.GetChapterAsync(novelId, chapterNumber, userId);
                if (chapter == null)
                {
                    return NotFound("Chapter not found");
                }

                var accessStatus = await _chapterAccessService.GetChapterAccessStatusAsync(userId, chapter.Id);
                if (!accessStatus.IsAccessible)
                {
                    chapter.Content = "This chapter is locked. Please unlock it to read.";
                    chapter.PdfPath = "";
                }

                return Ok(chapter);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Checks if a chapter is the user's current reading position
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="chapterNumber">The sequential number of the chapter</param>
        /// <returns>Boolean indicating whether this is the current chapter</returns>
        /// <response code="200">Returns true if current chapter, false otherwise</response>
        /// <response code="400">If there was an error checking the chapter status</response>
        /// <remarks>
        /// This endpoint is used to highlight the current reading position in the UI.
        /// </remarks>
        [HttpGet("is-current-chapter/{userId}/{novelId}/{chapterNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsCurrentChapter(int userId, int novelId, int chapterNumber)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("User ID is required");
                }

                int lastReadChapter = await _chapterRepository.GetLastReadChapterAsync(userId, novelId);
                bool isCurrentChapter = lastReadChapter == chapterNumber;
                
                return Ok(isCurrentChapter);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Toggles a chapter as the user's current reading position
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="chapterNumber">The sequential number of the chapter</param>
        /// <returns>Updated reading progress information</returns>
        /// <response code="200">Reading position updated successfully</response>
        /// <response code="400">If there was an error updating the reading position</response>
        /// <remarks>
        /// This endpoint works as a toggle:
        /// - If the chapter is already marked as last read, it will be unmarked
        /// - If a different chapter is marked, this one will be marked instead
        /// - If no chapter is marked, this one will be marked
        /// </remarks>
        [HttpPost("toggle-last-read/{userId}/{novelId}/{chapterNumber}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLastReadChapter(int userId, int novelId, int chapterNumber)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("User ID is required");
                }

                // Get current last read chapter
                int currentLastReadChapter = await _chapterRepository.GetLastReadChapterAsync(userId, novelId);
                
                // Toggle logic
                int newLastReadChapter = 0; // 0 means no chapter is marked as last read
                
                if (currentLastReadChapter != chapterNumber)
                {
                    // If different chapter, set new chapter as last read
                    newLastReadChapter = chapterNumber;
                }
                // If same chapter, leave newLastReadChapter as 0 to clear it
                
                // Update with new value
                bool success = await _chapterRepository.UpdateLastReadChapterAsync(userId, novelId, newLastReadChapter);
                
                if (success)
                {
                    return Ok(new { 
                        Success = true, 
                        LastReadChapter = newLastReadChapter,
                        IsMarked = newLastReadChapter > 0
                    });
                }
                
                return BadRequest("Failed to update last read chapter");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
