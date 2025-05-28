using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace BasicWebNovelAPI.Controllers
{
    /// <summary>
    /// Controller for handling user registration processes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IEmailRepository _emailRepository;
        private readonly IUserRepository _userRepository;

        public RegistrationController(
            IRegistrationRepository registrationRepository,
            ILogger<RegistrationController> logger,
            IEmailRepository emailRepository,
            IUserRepository userRepository)
        {
            _registrationRepository = registrationRepository;
            _logger = logger;
            _emailRepository = emailRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Registers a new user with email and password
        /// </summary>
        /// <param name="registerUserDto">Registration information including user credentials and profile</param>
        /// <returns>Details of the newly registered user</returns>
        /// <response code="200">User registered successfully. A verification code is sent to the user's email.</response>
        /// <response code="400">If registration information is invalid</response>
        /// <response code="404">If required data is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/register
        ///     Content-Type: multipart/form-data
        ///     
        ///     {
        ///         "email": "user@example.com",
        ///         "userName": "johnsmith",
        ///         "password": "SecurePassword123!",
        ///         "firstName": "John",
        ///         "lastName": "Smith",
        ///         "profileImage": [binary image file]
        ///     }
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto registerUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid registration data: {@ValidationErrors}",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));

                    return BadRequest(ModelState);
                }

                var registeredUser = await _registrationRepository.Registration(registerUserDto);
                return Ok(new
                {
                    user = registeredUser,
                    message = "Registration successful. Please check your email for verification code."
                });
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
        /// Verifies user email with the verification code
        /// </summary>
        /// <param name="verifyCodeDto">Object containing email, password and verification code</param>
        /// <returns>Verification result</returns>
        /// <response code="200">Email successfully verified</response>
        /// <response code="400">If verification information is invalid</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/verify-email
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "SecurePassword123!",
        ///         "temporaryCode": "123456"
        ///     }
        /// </remarks>
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyEmail(VerifyCodeDto verifyCodeDto)
        {
            try
            {
                var result = await _registrationRepository.VerifyEmail(verifyCodeDto);
                return Ok(new { verified = result, message = "Email successfully verified" });
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
        /// Registers a user with Facebook account information
        /// </summary>
        /// <param name="accessToken">Facebook access token</param>
        /// <returns>Registration success confirmation</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">If registration information is invalid or user already exists</response>
        /// <response code="404">If required data is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/facebook-registration
        ///     "EAAZAWJOXgj4UBAGXLcpxN4v6cqZCWuiZB..."
        ///     
        /// Sample response:
        ///
        ///     {
        ///         "message": "Registration successful"
        ///     }
        /// </remarks>
        [HttpPost("facebook-registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrationFacebook(string accessToken)
        {
            try
            {
                var result = await _registrationRepository.FaceBookRegister(accessToken);
                if (!result)
                    return BadRequest("User already registered");

                return Ok(new { message = "Registration successful" });
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
        /// Resends verification code to user's email
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>Status of verification code delivery</returns>
        /// <response code="200">Verification code sent successfully</response>
        /// <response code="400">If email is invalid</response>
        /// <response code="404">If user is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/resend-verification
        ///     "user@example.com"
        /// </remarks>
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendVerification(string email)
        {
            try
            {
                // Use the ForgotPasswordAsync method which already has similar functionality
                string result = await _userRepository.ForgotPasswordAsync(email);
                return Ok(new { message = "Verification code sent successfully" });
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
        /// Returns registration error information
        /// </summary>
        /// <param name="error">Error message from registration process</param>
        /// <returns>Formatted error object with redirect information</returns>
        /// <response code="200">Returns error details and redirect information</response>
        /// <remarks>
        /// Sample response:
        ///
        ///     {
        ///         "error": true,
        ///         "message": "Registration error: Email already in use",
        ///         "redirectTo": "/register"
        ///     }
        /// </remarks>
        [HttpGet("error")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Error(string error)
        {
            _logger.LogWarning("Registration error: {Error}", error);

            // Return a view-friendly response with JSON
            return Ok(new
            {
                error = true,
                message = $"Registration error: {error}",
                redirectTo = "/register" // Frontend can use this to redirect the user
            });
        }

        /// <summary>
        /// Confirms successful registration
        /// </summary>
        /// <returns>Registration success confirmation</returns>
        /// <response code="200">Returns success confirmation</response>
        /// <remarks>
        /// Sample response:
        ///
        ///     {
        ///         "success": true,
        ///         "message": "Registration successful"
        ///     }
        /// </remarks>
        [HttpGet("success")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Success()
        {
            return Ok(new { success = true, message = "Registration successful" });
        }
    }
}
