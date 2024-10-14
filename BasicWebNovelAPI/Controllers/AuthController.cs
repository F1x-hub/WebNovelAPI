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
        
        private readonly IEmailRepository _emailRepository;
        

        public AuthController(IAuthorizationRepository authorizationRepository,
                              IEmailRepository emailRepository)
        {
            _authorizationRepository = authorizationRepository;
            _emailRepository = emailRepository;
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

        

        [HttpPost("google-authorization")]
        public async Task<IActionResult> AuthorizationGoogle(string accessToken)
        {
            var token = await _authorizationRepository.GoogleAuthorization(accessToken);

            if (string.IsNullOrEmpty(token)) 
                return BadRequest("Not Authorized");

            return Ok($"Success: {token}");
        }

        

        [HttpPost("facebook-authorization")]
        public async Task<IActionResult> AuthorizationFacebook(string accessToken)
        {
            var token = await _authorizationRepository.FaceBookAuthorization(accessToken);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Not Authorized");

            return Ok($"Success: {token}");
        }


    }
}
