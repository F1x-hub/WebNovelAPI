using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing user's personal novel libraries
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserLibraryController : ControllerBase
    {
        private readonly IUserLibraryRepository _userLibraryRepository;

        public UserLibraryController(IUserLibraryRepository userLibraryRepository)
        {
            _userLibraryRepository = userLibraryRepository;
        }

        /// <summary>
        /// Retrieves all novels in a user's library
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>List of novels in the user's library with reading progress</returns>
        /// <response code="200">Returns the list of novels in the user's library</response>
        /// <response code="404">If user has no novels in their library</response>
        /// <response code="400">If there was an error retrieving the library</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// The library includes reading progress tracking for each novel.
        /// 
        /// Sample response:
        ///
        ///     [
        ///         {
        ///             "novelId": 1,
        ///             "title": "Fantasy Novel",
        ///             "author": "Author Name",
        ///             "lastReadChapter": 5,
        ///             "totalChapters": 20,
        ///             "dateAdded": "2023-05-20T14:30:45Z"
        ///         },
        ///         ...
        ///     ]
        /// </remarks>
        [HttpGet("user-library/{userId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserLibrary(int userId)
        {
            try 
            {
                var library = await _userLibraryRepository.GetUserLibraryAsync(userId);

                if (library == null || library.Count == 0)
                {
                    return NotFound("The user has no novels in their library.");
                }

                return Ok(library);
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
        /// Checks if a novel is in a user's library
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <returns>Boolean indicating whether the novel is in the user's library</returns>
        /// <response code="200">Returns true if novel is in library, false otherwise</response>
        /// <response code="400">If there was an error checking the library</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// 
        /// Sample response:
        ///
        ///     true
        /// </remarks>
        [HttpGet("check-novel/{userId}/{novelId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsNovelInUserLibrary(int userId, int novelId)
        {
            try
            {
                var result = await _userLibraryRepository.IsNovelInUserLibraryAsync(userId, novelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a novel to a user's library or updates reading progress
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="lastReadChapter">The chapter number last read by the user</param>
        /// <returns>Confirmation of library update</returns>
        /// <response code="200">Novel added to or removed from library successfully</response>
        /// <response code="400">If there was an error updating the library</response>
        /// <response code="404">If the user or novel is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// If the novel is already in the user's library, this acts as a toggle and will remove it.
        /// If providing an updated lastReadChapter, it will update the reading progress.
        /// 
        /// Sample request:
        ///
        ///     POST /api/UserLibrary/add-to-library/1/2
        ///     5
        /// </remarks>
        [HttpPost("add-to-library/{userId}/{novelId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddNovelToLibrary(int userId, int novelId, [FromBody] int lastReadChapter)
        {
            try 
            {
                var result = await _userLibraryRepository.AddNovelToUserLibraryAsync(userId, novelId, lastReadChapter);

                if (result)
                {
                    var isInLibrary = await _userLibraryRepository.IsNovelInUserLibraryAsync(userId, novelId);
                    if (isInLibrary)
                    {
                        return Ok("Novel added to the user's library successfully.");
                    }
                    else
                    {
                        return Ok("Novel removed from the user's library successfully.");
                    }
                }

                return BadRequest("Failed to update the user's library.");
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
        /// Updates the last read chapter for a novel in the user's library
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <param name="lastReadChapter">The chapter number last read by the user</param>
        /// <returns>Confirmation of chapter progress update</returns>
        /// <response code="200">Reading progress updated successfully</response>
        /// <response code="400">If there was an error updating the reading progress</response>
        /// <response code="404">If the user or novel is not found</response>
        [HttpPut("update-progress/{userId}/{novelId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReadingProgress(int userId, int novelId, [FromBody] int lastReadChapter)
        {
            try
            {
                var result = await _userLibraryRepository.UpdateLastReadChapterAsync(userId, novelId, lastReadChapter);
                
                if (result)
                {
                    return Ok("Reading progress updated successfully.");
                }
                
                return BadRequest("Failed to update reading progress.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Resets the AddedChapter flag for a novel in the user's library
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <returns>Confirmation of reset operation</returns>
        /// <response code="200">AddedChapter flag reset successfully</response>
        /// <response code="400">If there was an error resetting the flag</response>
        /// <response code="404">If the user or novel is not found</response>
        [HttpPut("reset-added-chapter/{userId}/{novelId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetAddedChapter(int userId, int novelId)
        {
            try
            {
                var result = await _userLibraryRepository.ResetAddedChapterAsync(userId, novelId);
                
                if (result)
                {
                    return Ok("AddedChapter flag reset successfully.");
                }
                
                return BadRequest("Failed to reset AddedChapter flag.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
