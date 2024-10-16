using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("add-to-library/{userId}/{novelId}")]
        public async Task<IActionResult> AddNovelToLibrary(int userId, int novelId, [FromBody] int lastReadChapter)
        {
            try 
            {
                var result = await _userLibraryRepository.AddNovelToUserLibraryAsync(userId, novelId, lastReadChapter);

                if (result)
                {
                    return Ok("Novel added to the user's library successfully.");
                }

                return BadRequest("Failed to add the novel to the user's library.");
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
