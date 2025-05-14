using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterController(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        [HttpPost("create-chapter/{userId}/{novelId}")]
        [Authorize(Roles = "User")]
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

        [HttpPut("update-chapter/{novelId}/{chapterId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
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


        [HttpDelete("delete-chapter/{novelId}/{chapterId}/{userId}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
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

        [HttpGet("novel-all-chapters/{novelId}")]
        public async Task<IActionResult> GetAllChapters(int novelId)
        {
            try
            {
                var chapters = await _chapterRepository.GetAllChaptersAsync(novelId);
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

        [HttpGet("get-chapter/{novelId}/{chapterNumber}/{userId?}")]
        public async Task<IActionResult> GetChapterByNumber(int novelId, int chapterNumber, int userId = 0)
        {
            try
            {
                var chapter = await _chapterRepository.GetChapterAsync(novelId, chapterNumber, userId);
                if (chapter == null)
                {
                    return NotFound("Chapter not found");
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

        [HttpGet("is-current-chapter/{userId}/{novelId}/{chapterNumber}")]
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

        [HttpPost("toggle-last-read/{userId}/{novelId}/{chapterNumber}")]
        //pp[Authorize(Roles = "User")]
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
