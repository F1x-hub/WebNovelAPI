using AutoMapper;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }


        [HttpGet("get-all-user")]
        [Authorize(Roles = "Admin")]
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



        [HttpGet("get-user/{id}")]
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



        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")]
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


        [HttpPut("update/{userId}")]
        [Authorize(Roles = "Admin,User")]
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
        
        [HttpPost("forgot-password")]
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
        
        [HttpPost("reset-password")]
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
        
        [HttpPost("change-password")]
        [Authorize(Roles = "Admin,User")]
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

        [HttpPost("set-user-as-adult/{userId}")]
        [Authorize(Roles = "Admin,User")]
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
