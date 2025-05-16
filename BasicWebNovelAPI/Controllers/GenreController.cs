using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Novel.Genre;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing novel genres
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository _genreRepository;

        public GenreController(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        /// <summary>
        /// Retrieves all available novel genres
        /// </summary>
        /// <returns>List of all genres with their details</returns>
        /// <response code="200">Returns the list of all genres</response>
        /// <response code="404">If no genres are found</response>
        /// <response code="400">If there was an error retrieving genres</response>
        /// <remarks>
        /// Sample response:
        ///
        ///     [
        ///         {
        ///             "id": 1,
        ///             "name": "Fantasy",
        ///             "description": "Fiction with fantastic elements"
        ///         },
        ///         {
        ///             "id": 2,
        ///             "name": "Science Fiction",
        ///             "description": "Fiction based on scientific discoveries"
        ///         }
        ///     ]
        /// </remarks>
        [HttpGet("get-all-genre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllGenre()
        {
            try
            {
                var genre = await _genreRepository.GetGenresAsync();

                if (genre == null || genre.Count == 0)
                {
                    return NotFound("genre not found");
                }

                return Ok(genre);
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
        /// Creates a new novel genre
        /// </summary>
        /// <param name="createGenreDto">Object containing the new genre details</param>
        /// <returns>The newly created genre with its assigned ID</returns>
        /// <response code="200">Returns the created genre</response>
        /// <response code="400">If the genre data is invalid</response>
        /// <response code="404">If required related data is not found</response>
        /// <remarks>
        /// This endpoint is restricted to administrators only
        /// 
        /// Sample request:
        ///
        ///     POST /api/Genre/create-genre
        ///     {
        ///         "name": "Mystery",
        ///         "description": "Fiction that involves solving a mystery or crime"
        ///     }
        /// </remarks>
        [HttpPost("create-genre")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreDto createGenreDto)
        {
            try
            {
                var createdGenre = await _genreRepository.CreateGenreAsync(createGenreDto);
                return Ok(createdGenre);
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
