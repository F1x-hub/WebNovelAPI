using AutoMapper;
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
            var users = await _userRepository.GetUsersAsync();
            if (users == null)
            {
                return NotFound("No users found.");
            }

            var getUserDto = _mapper.Map<IEnumerable<GetUserDto>>(users);
            return Ok(getUserDto);
        }



        [HttpGet("get-user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserId(int id)
        {
            var user = await _userRepository.GetUserIdAsync(id);
            if (user == null)
            {
                return NotFound("No users found.");
            }


            var getUserDto = _mapper.Map<GetUserDto>(user);


            return Ok(getUserDto);
        }



        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var deletedUser = await _userRepository.DeleteUserIdAsync(userId);
            if (deletedUser == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new { Message = "User deleted successfully." });
        }


        [HttpPut("update/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int userId, GetUserDto userDto)
        {
            var user = await _userRepository.GetUserIdAsync(userId);


            if (user == null)
            {
                return NotFound("User not found.");
            }


            _mapper.Map(userDto, user);


            bool isUpdated = await _userRepository.UpdateUserAsync(user);

            if (!isUpdated)
            {
                return BadRequest("Failed to update the user.");
            }


            return Ok(user);
        }
    }
}
