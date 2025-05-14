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
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingController(IRatingRepository ratingRepository) 
        {
            _ratingRepository = ratingRepository;
        }

        [HttpPost("rate-novel/{novelId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        public async Task<IActionResult> RateNovel(int novelId, int userId, [FromBody] double ratingValue)
        {
            try 
            {
                var result = await _ratingRepository.RateNovelAsync(novelId, userId, ratingValue);

                if (!result)
                {
                    return BadRequest("Invalid rating. Rating must be between 1 and 5.");
                }

                return Ok("Rating submitted successfully.");
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

        [HttpGet("get-novel-rating/{novelId}")]
        public async Task<IActionResult> GetNovelRating(int novelId)
        {
            try 
            {
                var averageRating = await _ratingRepository.GetNovelRatingAsync(novelId);

                return Ok(new { NovelId = novelId, AverageRating = averageRating });
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
        
        [HttpGet("most-popular-last-week")]
        public async Task<IActionResult> GetMostPopularNovelsLastWeek(int limit = 10)
        {
            try 
            {
                var novels = await _ratingRepository.GetMostPopularNovelsLastWeekAsync(limit);
                
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
        
        [HttpGet("by-rating")]
        public async Task<IActionResult> GetNovelsByRating(int limit = 10)
        {
            try 
            {
                var novels = await _ratingRepository.GetNovelsByRatingAsync(limit);
                
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
    }
}
