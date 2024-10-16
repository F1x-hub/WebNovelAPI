using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImageController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpPost("add-novel-image/{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UploadNovelImages(int id, IFormFile? imageFiles)
        {
            try
            {
                await _imageRepository.AddNovelImagesAsync(id, imageFiles);

                return Ok(new { Message = "Images uploaded successfully!" });
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

        [HttpPost("add-user-image/{id}")]
        public async Task<IActionResult> UploadUserImages(int id, IFormFile? imageFiles)
        {
            try
            {
                await _imageRepository.AddUserImagesAsync(id, imageFiles);

                return Ok(new { Message = "Images uploaded successfully!" });
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
