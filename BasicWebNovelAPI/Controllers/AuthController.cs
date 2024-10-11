using AutoMapper;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IMapper _mapper;

        public AuthController(IAuthorizationRepository authorizationRepository,
                              IMapper mapper,
                              IImageRepository imageRepository,
                              IEmailRepository emailRepository)
        {
            _authorizationRepository = authorizationRepository;
            _mapper = mapper;
            _imageRepository = imageRepository;
            _emailRepository = emailRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto registerUserDto)
        {
            try
            {

                var registeredUser = await _authorizationRepository.Registration(registerUserDto);


                return Ok(registeredUser);
            }
            catch (Exception ex)
            {

                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("add-user-image/{id}")]
        public async Task<IActionResult> UploadUserImages(int id, IFormFile? imageFiles)
        {
            try
            {
                await _authorizationRepository.AddUserImagesAsync(id, imageFiles);

                return Ok(new { Message = "Images uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> LogIn(GetLoginDto loginDto)
        {
            string response = await _authorizationRepository.LogIn(loginDto);
            
            return Ok($"{response}");
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            string token = await _authorizationRepository.VerifyCode(verifyCodeDto);

            return Ok($"User successfully logged in. Token: {token}");
        }

        [HttpPost("google-registration")]
        public async Task<IActionResult> RegistrationGoogle(string accessToken)
        {
            var result = await _authorizationRepository.GoogleRegister(accessToken);

            if (!result)
                return BadRequest("Not Registered");

            return Ok("Success");
        }

        [HttpPost("google-authorization")]
        public async Task<IActionResult> AuthorizationGoogle(string accessToken)
        {
            var token = await _authorizationRepository.GoogleAuthorization(accessToken);

            if (string.IsNullOrEmpty(token)) 
                return BadRequest("Not Authorized");

            return Ok($"Success: {token}");
        }

        [HttpPost("facebook-registration")]
        public async Task<IActionResult> RegistrationFacebook(string accessToken)
        {
            var result = await _authorizationRepository.FaceBookRegister(accessToken);
            if (!result)
                return BadRequest("Not Registered");

            return Ok("Success");
        }

        [HttpPost("facebook-authorization")]
        public async Task<IActionResult> AuthorizationFacebook(string accessToken)
        {
            var token = await _authorizationRepository.FaceBookAuthorization(accessToken);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Not Authorized");

            return Ok($"Success: {token}");
        }


        

        [HttpGet("get-all-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authorizationRepository.GetUsers();
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
            var user = await _authorizationRepository.GetUserId(id);
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
            var deletedUser = await _authorizationRepository.DeleteUserId(userId);
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
            var user = await _authorizationRepository.GetUserId(userId);


            if (user == null)
            {
                return NotFound("User not found.");
            }


            _mapper.Map(userDto, user);


            bool isUpdated = await _authorizationRepository.UpdateUser(user);

            if (!isUpdated)
            {
                return BadRequest("Failed to update the user.");
            }


            return Ok(user);
        }
    }
}
