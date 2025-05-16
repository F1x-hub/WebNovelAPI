using AutoMapper;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for managing user accounts and profiles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all users in the system
        /// </summary>
        /// <returns>List of all users with their profile information</returns>
        /// <response code="200">Returns the list of all users</response>
        /// <response code="404">If no users are found</response>
        /// <response code="400">If there was an error retrieving users</response>
        /// <remarks>
        /// This endpoint is restricted to administrators only
        /// </remarks>
        [HttpGet("get-all-user")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsers()
        {
            try 
            {
                var users = await _userRepository.GetUsersAsync();
                if (users == null)
                {
                    return NotFound("No users found.");
                }

                var getUserDto = _mapper.Map<IEnumerable<GetUserDto>>(users);
                return Ok(getUserDto);
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
        /// Retrieves a user by their ID
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>User profile information</returns>
        /// <response code="200">Returns the user's profile</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="400">If there was an error retrieving the user</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/User/get-user/1
        ///
        /// Sample response:
        ///
        ///     {
        ///         "id": 1,
        ///         "userName": "john_doe",
        ///         "email": "john@example.com",
        ///         "firstName": "John",
        ///         "lastName": "Doe",
        ///         "isAdult": true,
        ///         "role": "User"
        ///     }
        /// </remarks>
        [HttpGet("get-user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserId(int id)
        {
            try 
            {
                var user = await _userRepository.GetUserIdAsync(id);
                if (user == null)
                {
                    return NotFound("No users found.");
                }


                var getUserDto = _mapper.Map<GetUserDto>(user);


                return Ok(getUserDto);
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
        /// Deletes a user account
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete</param>
        /// <returns>Confirmation of successful deletion</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="400">If there was an error deleting the user</response>
        /// <remarks>
        /// This endpoint is restricted to administrators only
        /// </remarks>
        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try 
            {
                var deletedUser = await _userRepository.DeleteUserIdAsync(userId);
                if (deletedUser == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(new { Message = "User deleted successfully." });
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
        /// Updates a user's profile information
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update</param>
        /// <param name="userDto">Object containing the updated user information</param>
        /// <returns>Confirmation of successful update</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="400">If there was an error updating the user</response>
        /// <remarks>
        /// This endpoint is restricted to the user themselves or administrators
        /// 
        /// Sample request:
        ///
        ///     PUT /api/User/update/1
        ///     {
        ///         "firstName": "John",
        ///         "lastName": "Smith",
        ///         "userName": "john_smith"
        ///     }
        /// </remarks>
        [HttpPut("update/{userId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto userDto)
        {
            try 
            {
                bool isUpdated = await _userRepository.UpdateUserAsync(userId, userDto);

                if (!isUpdated)
                {
                    return NotFound("User not found or failed to update.");
                }

                return Ok("User updated successfully.");
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
        /// Initiates password reset process for a forgotten password
        /// </summary>
        /// <param name="email">Email address of the user requesting password reset</param>
        /// <returns>Confirmation message that reset email has been sent</returns>
        /// <response code="200">Reset email sent successfully</response>
        /// <response code="400">If there was an error processing the request</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/User/forgot-password
        ///     "user@example.com"
        /// </remarks>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                var result = await _userRepository.ForgotPasswordAsync(email);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Resets a user's password using a reset token
        /// </summary>
        /// <param name="resetPasswordDto">Object containing email, reset token, and new password</param>
        /// <returns>Confirmation of successful password reset</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">If the reset token is invalid or expired</response>
        /// <response code="404">If the user is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/User/reset-password
        ///     {
        ///         "email": "user@example.com",
        ///         "token": "reset-token-from-email",
        ///         "newPassword": "NewPassword123!"
        ///     }
        /// </remarks>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                bool isReset = await _userRepository.ResetPasswordAsync(resetPasswordDto);
                if (isReset)
                {
                    return Ok(new { Message = "Password has been reset successfully." });
                }
                return BadRequest("Failed to reset password.");
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
        /// Changes a user's password (requires current password)
        /// </summary>
        /// <param name="changePasswordDto">Object containing user ID, current password, and new password</param>
        /// <returns>Confirmation of successful password change</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">If the current password is incorrect</response>
        /// <response code="404">If the user is not found</response>
        /// <remarks>
        /// This endpoint is restricted to the user themselves or administrators
        /// 
        /// Sample request:
        ///
        ///     POST /api/User/change-password
        ///     {
        ///         "userId": 1,
        ///         "currentPassword": "OldPassword123!",
        ///         "newPassword": "NewPassword123!"
        ///     }
        /// </remarks>
        [HttpPost("change-password")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                bool isChanged = await _userRepository.ChangePasswordAsync(changePasswordDto);
                if (isChanged)
                {
                    return Ok(new { Message = "Password has been changed successfully." });
                }
                return BadRequest("Failed to change password.");
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
        /// Sets a user's age verification status to adult (18+)
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Confirmation of successful update</returns>
        /// <response code="200">User marked as adult successfully</response>
        /// <response code="400">If there was an error updating the status</response>
        /// <remarks>
        /// This endpoint is restricted to the user themselves or administrators.
        /// Used to grant access to adult content.
        /// </remarks>
        [HttpPost("set-user-as-adult/{userId}")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetUserAsAdult(int userId)
        {
            try
            {
                var result = await _userRepository.SetUserAsAdultAsync(userId);
                if (result)
                    return Ok(new { Message = "User has been successfully marked as an adult" });
                    
                return BadRequest("Failed to update user age status");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
