using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BasicWebNovelAPI.Data;
using Microsoft.EntityFrameworkCore;
using BasicWebNovelAPI.Enum;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing web novels
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class NovelController : ControllerBase
    {
        private readonly INovelRepository _novelRepository;
        private readonly BasicWebNovelContext _context;

        public NovelController(INovelRepository novelRepository, BasicWebNovelContext context)
        {
            _novelRepository = novelRepository;
            _context = context;
        }

        /// <summary>
        /// Retrieves a paginated list of novels with optional filtering
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of results per page (default: 10)</param>
        /// <param name="genreId">Optional filter by genre ID</param>
        /// <param name="status">Optional filter by novel status (In Progress, Completed, Hiatus)</param>
        /// <param name="sortBy">Optional sorting parameter (e.g., "title", "date", "rating")</param>
        /// <param name="userId">Optional user ID to filter adult content (0 means unauthenticated)</param>
        /// <returns>Paginated list of novels matching criteria</returns>
        /// <response code="200">Returns the list of novels</response>
        /// <response code="404">If no novels are found matching criteria</response>
        /// <response code="400">If there was an error retrieving novels</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Novel/get-all-novels?pageNumber=1&amp;pageSize=10&amp;genreId=2&amp;status=Completed&amp;sortBy=rating
        /// </remarks>
        [HttpGet("get-all-novels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNovels(
            int pageNumber = 1, 
            int pageSize = 10, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int userId = 0)
        {
            try 
            {
                var result = await _novelRepository.GetNovels(pageNumber, pageSize, genreId, status, sortBy);
                if (result == null || result.Novels == null || !result.Novels.Any())
                {
                    return NotFound("Novel not found.");
                }
                
                // No filtering of adult content - return all novels regardless of IsAdultContent status
                
                return Ok(result);
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
        /// Retrieves a specific novel by ID and increments view count
        /// </summary>
        /// <param name="id">The unique identifier of the novel</param>
        /// <param name="userId">Optional user ID to check adult content access (0 means unauthenticated)</param>
        /// <returns>Detailed information about the requested novel</returns>
        /// <response code="200">Returns the novel details</response>
        /// <response code="404">If novel is not found</response>
        /// <response code="400">If there was an error retrieving the novel</response>
        /// <response code="401">If authentication is required for adult content</response>
        /// <response code="403">If user is not authorized to access adult content</response>
        /// <remarks>
        /// This endpoint automatically increments the view count for the novel.
        /// Adult content is restricted to authenticated users who are verified as adults.
        /// </remarks>
        [HttpGet("get-novel/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetNovelById(int id, [FromQuery] int userId = 0)
        {
            try 
            {
                // Get client IP address
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                // First get novel data to check adult content
                var novel = await _novelRepository.GetNovelById(id);
                if (novel == null)
                {
                    return NotFound("Novel not found.");
                }
                
                // Check adult content restrictions
                if (novel.IsAdultContent)
                {
                    // For adult content, user must be authenticated
                    if (userId <= 0)
                    {
                        return Unauthorized("Authentication required to access adult content.");
                    }
                    
                    // Check if user is of appropriate age
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.IsAdult)
                    {
                        return Forbid("You must be 18+ to access this content.");
                    }
                }
                
                // Create the response first
                var result = Ok(novel);
                
                // Then increment views asynchronously (this operation can happen after response is sent)
                // This helps avoid transaction issues
                try
                {
                    _ = _novelRepository.IncrementNovelViews(id, userId, ipAddress!);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the request
                    Console.WriteLine($"Failed to increment view count: {ex.Message}");
                }
                
                return result;
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
        /// Searches novels by name with optional filtering
        /// </summary>
        /// <param name="name">Search query for novel title</param>
        /// <param name="genreId">Optional filter by genre ID</param>
        /// <param name="status">Optional filter by novel status (In Progress, Completed, Hiatus)</param>
        /// <param name="sortBy">Optional sorting parameter (e.g., "title", "date", "rating")</param>
        /// <param name="userId">Optional user ID to filter adult content (0 means unauthenticated)</param>
        /// <returns>List of novels matching the search criteria</returns>
        /// <response code="200">Returns novels matching the search</response>
        /// <response code="404">If no novels match the criteria</response>
        /// <response code="400">If there was an error processing the search</response>
        /// <remarks>
        /// Adult content is automatically filtered based on user authentication status and age verification.
        /// Unauthenticated users will not see adult content in results.
        /// 
        /// Sample request:
        ///
        ///     GET /api/Novel/get-novel-by-name?name=dragon&amp;genreId=2&amp;status=Completed
        /// </remarks>
        [HttpGet("get-novel-by-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNovelByName(
            string name, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int userId = 0)
        {
            try 
            {
                var novels = await _novelRepository.GetNovelByName(name, genreId, status, sortBy);
                if (novels == null || !novels.Any())
                {
                    return NotFound("Novel not found.");
                }

                // Filter adult content based on user authentication and age
                if (userId <= 0)
                {
                    // For unauthenticated users, filter out adult content
                    novels = novels.Where(n => !n.IsAdultContent).ToList();
                }
                else
                {
                    // For authenticated users, check age restriction
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.IsAdult)
                    {
                        // Filter out adult content for users who are not adults
                        novels = novels.Where(n => !n.IsAdultContent).ToList();
                    }
                }
                
                if (!novels.Any())
                {
                    return NotFound("No novels matching your criteria were found.");
                }

                return Ok(novels);
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
        /// Retrieves all novels created by a specific user
        /// </summary>
        /// <param name="id">The unique identifier of the user whose novels to retrieve</param>
        /// <param name="genreId">Optional filter by genre ID</param>
        /// <param name="status">Optional filter by novel status (In Progress, Completed, Hiatus)</param>
        /// <param name="sortBy">Optional sorting parameter (e.g., "title", "date", "rating")</param>
        /// <param name="requestUserId">Optional ID of the requesting user (0 means unauthenticated)</param>
        /// <returns>List of novels created by the specified user</returns>
        /// <response code="200">Returns the list of user's novels</response>
        /// <response code="404">If no novels are found for this user</response>
        /// <response code="400">If there was an error retrieving the novels</response>
        /// <remarks>
        /// Adult content filtering is applied based on requestUserId's authentication status and age verification,
        /// unless the requestUserId matches the target user id (authors can always see their own content).
        /// </remarks>
        [HttpGet("get-all-user-novel/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllUserNovel(
            int id, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int requestUserId = 0)
        {
            try 
            {
                var novels = await _novelRepository.GetUserAllNovel(id, genreId, status, sortBy);
                if (novels == null || !novels.Any())
                {
                    return NotFound("Novel not found.");
                }

                // If the requesting user is not the owner of these novels
                if (requestUserId != id)
                {
                    // Filter adult content based on user authentication and age
                    if (requestUserId <= 0)
                    {
                        // For unauthenticated users, filter out adult content
                        novels = novels.Where(n => !n.IsAdultContent).ToList();
                    }
                    else
                    {
                        // For authenticated users, check age restriction
                        var user = await _context.Users.FindAsync(requestUserId);
                        if (user == null || !user.IsAdult)
                        {
                            // Filter out adult content for users who are not adults
                            novels = novels.Where(n => !n.IsAdultContent).ToList();
                        }
                    }
                    
                    if (!novels.Any())
                    {
                        return NotFound("No novels matching your criteria were found.");
                    }
                }

                return Ok(novels);
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
        /// Creates a new novel for a specified user
        /// </summary>
        /// <param name="createNovelDto">Object containing the new novel details</param>
        /// <param name="userId">The unique identifier of the user creating the novel</param>
        /// <returns>The newly created novel with its assigned ID</returns>
        /// <response code="200">Returns the created novel</response>
        /// <response code="400">If the novel data is invalid or user is not allowed to create adult content</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" role.
        /// To mark a novel as adult content, the user must be verified as an adult (18+).
        /// 
        /// Sample request:
        ///
        ///     POST /api/Novel/create-novel/1
        ///     {
        ///         "title": "My Amazing Novel",
        ///         "description": "A fantastic story of adventure",
        ///         "genreIds": [1, 2],
        ///         "isAdultContent": false,
        ///         "status": "InProgress"
        ///     }
        /// </remarks>
        [HttpPost("create-novel/{userId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateNovel([FromBody] CreateNovelDto createNovelDto, int userId)
        {
            try 
            {
                // If novel is marked as adult content, verify the user is an adult
                if (createNovelDto.IsAdultContent)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.IsAdult)
                    {
                        return BadRequest("You must be 18+ to create adult content.");
                    }
                }
                
                var createdNovel = await _novelRepository.CreateNovel(createNovelDto, userId);
                return Ok(createdNovel);
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
        /// Updates an existing novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel to update</param>
        /// <param name="userId">The unique identifier of the user making the update</param>
        /// <param name="updateNovelDto">Object containing the updated novel information</param>
        /// <returns>Confirmation of successful update</returns>
        /// <response code="200">Novel updated successfully</response>
        /// <response code="404">If novel is not found</response>
        /// <response code="400">If update data is invalid or user is not allowed to mark as adult content</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" role.
        /// Users can only update novels they have created, unless they are administrators.
        /// To mark a novel as adult content, the user must be verified as an adult (18+).
        /// 
        /// Sample request:
        ///
        ///     PUT /api/Novel/update-novel/1/2
        ///     {
        ///         "title": "Updated Novel Title",
        ///         "description": "New description for the novel",
        ///         "isAdultContent": false,
        ///         "status": "Completed"
        ///     }
        /// </remarks>
        [HttpPut("update-novel/{novelId}/{userId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNovel(int novelId, int userId, [FromBody] UpdateNovelDto updateNovelDto)
        {
            try 
            {
                // If setting novel to adult content, verify the user is an adult
                if (updateNovelDto.IsAdultContent.HasValue && updateNovelDto.IsAdultContent.Value)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.IsAdult)
                    {
                        return BadRequest("You must be 18+ to mark content as adult-only.");
                    }
                }
                
                bool isUpdated = await _novelRepository.UpdateNovel(novelId, userId, updateNovelDto);
                if (!isUpdated)
                {
                    return NotFound("Novel not found.");
                }

                return Ok("Novel updated successfully.");
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
        /// Deletes a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel to delete</param>
        /// <param name="userId">The unique identifier of the user requesting deletion</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Novel deleted successfully</response>
        /// <response code="404">If novel is not found</response>
        /// <response code="400">If there was an error deleting the novel</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "Admin" or "User" role.
        /// Users can only delete novels they have created, unless they are administrators.
        /// </remarks>
        [HttpDelete("delete-novel/{novelId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteNovel(int novelId, int userId)
        {
            try 
            {
                bool isDeleted = await _novelRepository.DeleteNovel(novelId, userId);
                if (!isDeleted)
                {
                    return NotFound("Novel not found.");
                }

                return Ok(new { Message = "Novel deleted successfully." });
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
        /// Increments the view count for a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="userId">Optional user ID for tracking (0 means anonymous view)</param>
        /// <returns>Confirmation of view count increment</returns>
        /// <response code="200">View count incremented successfully</response>
        /// <response code="404">If novel is not found</response>
        /// <response code="400">If there was an error incrementing the view count</response>
        /// <remarks>
        /// The system tracks IP addresses to prevent duplicate views in a short time period.
        /// This endpoint is typically called automatically when a novel is viewed,
        /// but can be called manually if needed.
        /// </remarks>
        [HttpPost("increment-views/{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IncrementViews(int novelId, [FromQuery] int userId = 0)
        {
            try
            {
                // Get client IP address
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                bool success = await _novelRepository.IncrementNovelViews(novelId, userId, ipAddress!);
                if (!success)
                {
                    return NotFound("Novel not found.");
                }
                
                return Ok(new { Message = "Views incremented successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
