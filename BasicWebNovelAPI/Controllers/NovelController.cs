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
    [Route("api/[controller]")]
    [ApiController]
    public class NovelController : ControllerBase
    {
        private readonly INovelRepository _novelRepository;
        private readonly BasicWebNovelContext _context;

        public NovelController(INovelRepository novelRepository, BasicWebNovelContext context)
        {
            _novelRepository = novelRepository;
            _context = context;
        }

        
        [HttpGet("get-all-novels")]
        public async Task<IActionResult> GetNovels(
            int pageNumber = 1, 
            int pageSize = 10, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string sortBy = null,
            [FromQuery] int userId = 0)
        {
            try 
            {
                var novels = await _novelRepository.GetNovels(pageNumber, pageSize, genreId, status, sortBy);
                if (novels == null || !novels.Any())
                {
                    return NotFound("Novel not found.");
                }
                
                // No filtering of adult content - return all novels regardless of IsAdultContent status
                
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

        
        [HttpGet("get-novel/{id}")]
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
                
                // Increment views
                bool viewIncremented = await _novelRepository.IncrementNovelViews(id, userId, ipAddress!);
                if (!viewIncremented)
                {
                    return NotFound("Novel not found.");
                }
                
                return Ok(novel);
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

        [HttpGet("get-novel-by-name")]
        public async Task<IActionResult> GetNovelByName(
            string name, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string sortBy = null,
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

        [HttpGet("get-all-user-novel/{id}")]
        public async Task<IActionResult> GetAllUserNovel(
            int id, 
            [FromQuery] int? genreId = null,
            [FromQuery] NovelStatus? status = null,
            [FromQuery] string sortBy = null,
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

        
        [HttpPost("create-novel/{userId}")]
        [Authorize(Roles = "User")]
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

        [HttpPut("update-novel/{novelId}/{userId}")]
        [Authorize(Roles = "User")]
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


        [HttpDelete("delete-novel/{novelId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
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
        
        [HttpPost("increment-views/{novelId}")]
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
