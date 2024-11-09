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
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository _genreRepository;

        public GenreController(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        [HttpGet("get-all-genre")]
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

        [HttpPost("create-genre")]
        [Authorize(Roles = "Admin")]
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
