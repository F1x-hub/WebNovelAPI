using AutoMapper;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for handling user authentication and authorization operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly ILogger<AuthController> _logger;
        

        public AuthController(IAuthorizationRepository authorizationRepository,
                              IRegistrationRepository registrationRepository,
                              IEmailRepository emailRepository,
                              ILogger<AuthController> logger)
        {
            _authorizationRepository = authorizationRepository;
            _registrationRepository = registrationRepository;
            _emailRepository = emailRepository;
            _logger = logger;
        }

        

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>JWT token upon successful authentication</returns>
        /// <response code="200">Returns JWT token for the user</response>
        /// <response code="400">If credentials are invalid</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/login
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "Password123!"
        ///     }
        ///     
        /// Sample response:
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///     }
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LogIn(GetLoginDto loginDto)
        {
            try
            {
                string token = await _authorizationRepository.LogIn(loginDto);
                return Ok(new { token });
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
        /// Verifies a 2FA code for user authentication
        /// </summary>
        /// <param name="verifyCodeDto">Object containing user email and verification code</param>
        /// <returns>JWT token upon successful verification</returns>
        /// <response code="200">Returns JWT token for the user after successful verification</response>
        /// <response code="400">If the code is invalid</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/verify-code
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "123456"
        ///     }
        ///     
        /// Sample response:
        /// 
        ///     "User successfully logged in. Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        /// </remarks>
        [HttpPost("verify-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Authenticates a user with Facebook access token
        /// </summary>
        /// <param name="accessToken">Facebook access token</param>
        /// <returns>JWT token upon successful authentication</returns>
        /// <response code="200">Returns JWT token for the user</response>
        /// <response code="400">If token is invalid or authentication fails</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/facebook-authorization
        ///     "EAAZAWJOXgj4UBAGXLcpxN4v6cqZCWuiZB..."
        ///     
        /// Sample response:
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///     }
        /// </remarks>
        [HttpPost("facebook-authorization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AuthorizationFacebook(string accessToken)
        {
            try
            {
                var token = await _authorizationRepository.FaceBookAuthorization(accessToken);
                if (string.IsNullOrEmpty(token))
                    return BadRequest("Not Authorized");

                return Ok(new { token });
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
        /// Returns authentication error information
        /// </summary>
        /// <param name="error">Error message from authentication process</param>
        /// <returns>Formatted error object with redirect information</returns>
        /// <response code="200">Returns error details and redirect information</response>
        /// <remarks>
        /// Sample response:
        /// 
        ///     {
        ///         "error": true,
        ///         "message": "Authentication error: Invalid token",
        ///         "redirectTo": "/login"
        ///     }
        /// </remarks>
        [HttpGet("error")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Error(string error)
        {
            _logger.LogWarning("Authentication error: {Error}", error);
            
            // Return a view-friendly response with JSON
            return Ok(new { 
                error = true,
                message = $"Authentication error: {error}",
                redirectTo = "/login" // Frontend can use this to redirect the user
            });
        }
    }
}
