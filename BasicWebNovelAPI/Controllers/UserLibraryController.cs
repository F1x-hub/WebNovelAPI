using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLibraryController : ControllerBase
    {
        private readonly IUserLibraryRepository _userLibraryRepository;

        public UserLibraryController(IUserLibraryRepository userLibraryRepository)
        {
            _userLibraryRepository = userLibraryRepository;
        }

        [HttpGet("user-library/{userId}")]
        [Authorize(Roles = "User")]
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

        [HttpGet("check-novel/{userId}/{novelId}")]
        [Authorize(Roles = "User")]
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

        [HttpPost("add-to-library/{userId}/{novelId}")]
        [Authorize(Roles = "User")]
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
    }
}
