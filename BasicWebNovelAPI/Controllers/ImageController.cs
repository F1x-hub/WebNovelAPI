using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing novel and user profile images
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImageController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        /// <summary>
        /// Uploads a cover image for a novel
        /// </summary>
        /// <param name="id">The unique identifier of the novel</param>
        /// <param name="imageFiles">The image file to upload (supports JPG, PNG formats)</param>
        /// <returns>Confirmation of successful upload</returns>
        /// <response code="200">Image uploaded successfully</response>
        /// <response code="400">If the file is invalid or too large</response>
        /// <response code="404">If the novel is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users who own the novel or administrators.
        /// The image will be resized and optimized automatically.
        /// </remarks>
        [HttpPost("add-novel-image/{id}")]
        [Authorize(Roles = "Admin, User")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Uploads a profile image for a user
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <param name="imageFiles">The image file to upload (supports JPG, PNG formats)</param>
        /// <returns>Confirmation of successful upload</returns>
        /// <response code="200">Image uploaded successfully</response>
        /// <response code="400">If the file is invalid or too large</response>
        /// <response code="404">If the user is not found</response>
        /// <remarks>
        /// The image will be resized and optimized automatically for profile display.
        /// </remarks>
        [HttpPost("add-user-image/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves a novel's cover image
        /// </summary>
        /// <param name="id">The unique identifier of the novel</param>
        /// <returns>The novel's cover image file</returns>
        /// <response code="200">Returns the image file</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the novel or image is not found</response>
        [HttpGet("get-novel-image/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("image/jpeg", "image/png")]
        public async Task<IActionResult> GetNovelImage(int id)
        {
            try
            {
                var imageStream = await _imageRepository.GetNovelImageAsync(id);
                return File(imageStream, "image/jpeg");
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
        /// Retrieves a user's profile image
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>The user's profile image file</returns>
        /// <response code="200">Returns the image file</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the user or image is not found</response>
        [HttpGet("get-user-image/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("image/jpeg", "image/png")]
        public async Task<IActionResult> GetUserImage(int id)
        {
            try
            {
                var imageStream = await _imageRepository.GetUserImageAsync(id);
                return File(imageStream, "image/jpeg");
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
        /// Deletes a novel's cover image
        /// </summary>
        /// <param name="id">The unique identifier of the novel</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Image deleted successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the novel or image is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users who own the novel or administrators.
        /// </remarks>
        [HttpDelete("delete-novel-image/{id}")]
        [Authorize(Roles = "Admin, User")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNovelImage(int id)
        {
            try
            {
                await _imageRepository.DeleteNovelImageAsync(id);
                return Ok(new { Message = "Image deleted successfully!" });
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
        /// Deletes a user's profile image
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">Image deleted successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the user or image is not found</response>
        /// <remarks>
        /// This endpoint is restricted to authenticated users.
        /// Users can only delete their own profile images unless they are administrators.
        /// </remarks>
        [HttpDelete("delete-user-image/{id}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUserImage(int id)
        {
            try
            {
                await _imageRepository.DeleteUserImageAsync(id);
                return Ok(new { Message = "Image deleted successfully!" });
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
