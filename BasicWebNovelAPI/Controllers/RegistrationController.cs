using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRepository _registrationRepository;

        public RegistrationController(IRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto registerUserDto)
        {
            try
            {

                var registeredUser = await _registrationRepository.Registration(registerUserDto);


                return Ok(registeredUser);
            }
            catch (Exception ex)
            {

                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("google-registration")]
        public async Task<IActionResult> RegistrationGoogle(string accessToken)
        {
            var result = await _registrationRepository.GoogleRegister(accessToken);

            if (!result)
                return BadRequest("Not Registered");

            return Ok("Success");
        }

        [HttpPost("facebook-registration")]
        public async Task<IActionResult> RegistrationFacebook(string accessToken)
        {
            var result = await _registrationRepository.FaceBookRegister(accessToken);
            if (!result)
                return BadRequest("Not Registered");

            return Ok("Success");
        }
    }
}
