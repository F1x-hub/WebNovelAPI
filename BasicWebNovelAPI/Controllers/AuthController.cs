using AutoMapper;
using BasicWebNovelAPI.Exceptions;
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
            try
            {
                string response = await _authorizationRepository.LogIn(loginDto);
                return Ok($"{response}");
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

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            try
            {
                string token = await _authorizationRepository.VerifyCode(verifyCodeDto);
                return Ok($"User successfully logged in. Token: {token}");
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

        

        [HttpPost("google-authorization")]
        public async Task<IActionResult> AuthorizationGoogle(string accessToken)
        {
            try
            {
                var token = await _authorizationRepository.GoogleAuthorization(accessToken);
                if (string.IsNullOrEmpty(token))
                    return BadRequest("Not Authorized");

                return Ok($"Success: {token}");
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

        

        [HttpPost("facebook-authorization")]
        public async Task<IActionResult> AuthorizationFacebook(string accessToken)
        {
            try
            {
                var token = await _authorizationRepository.FaceBookAuthorization(accessToken);
                if (string.IsNullOrEmpty(token))
                    return BadRequest("Not Authorized");

                return Ok($"Success: {token}");
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
