using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing novel ratings and popularity metrics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingController(IRatingRepository ratingRepository) 
        {
            _ratingRepository = ratingRepository;
        }

        /// <summary>
        /// Allows a user to rate a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel to rate</param>
        /// <param name="userId">The unique identifier of the user submitting the rating</param>
        /// <param name="ratingValue">Rating value (1.0 to 5.0)</param>
        /// <returns>Confirmation of rating submission</returns>
        /// <response code="200">Rating submitted successfully</response>
        /// <response code="400">If rating value is invalid (must be between 1 and 5)</response>
        /// <response code="404">If novel or user is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users with the "User" role.
        /// If the user has already rated this novel, the previous rating will be updated.
        /// 
        /// Sample request:
        ///
        ///     POST /api/Rating/rate-novel/1/2
        ///     4.5
        /// </remarks>
        [HttpPost("rate-novel/{novelId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves the average rating for a novel
        /// </summary>
        /// <param name="novelId">The unique identifier of the novel</param>
        /// <returns>Novel ID and its average rating</returns>
        /// <response code="200">Returns the novel ID and average rating</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the novel is not found</response>
        /// <remarks>
        /// The rating is calculated as the average of all user ratings for this novel.
        /// 
        /// Sample response:
        ///
        ///     {
        ///         "novelId": 1,
        ///         "averageRating": 4.2
        ///     }
        /// </remarks>
        [HttpGet("get-novel-rating/{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        
        /// <summary>
        /// Retrieves the most popular novels from the last week
        /// </summary>
        /// <param name="limit">Maximum number of novels to return (default: 10)</param>
        /// <returns>List of most popular novels based on recent views and ratings</returns>
        /// <response code="200">Returns the list of popular novels</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If no novels are found</response>
        /// <remarks>
        /// Popularity is calculated based on a combination of view counts and ratings within the last 7 days.
        /// </remarks>
        [HttpGet("most-popular-last-week")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        
        /// <summary>
        /// Retrieves top-rated novels
        /// </summary>
        /// <param name="limit">Maximum number of novels to return (default: 10)</param>
        /// <returns>List of novels sorted by highest average rating</returns>
        /// <response code="200">Returns the list of top-rated novels</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If no novels are found</response>
        /// <remarks>
        /// The list is sorted by average rating in descending order.
        /// Only novels with a minimum number of ratings are included to ensure statistical significance.
        /// </remarks>
        [HttpGet("by-rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
