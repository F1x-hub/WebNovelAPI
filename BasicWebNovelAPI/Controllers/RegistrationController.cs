using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
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

        public RegistrationController(
            IRegistrationRepository registrationRepository,
            ILogger<RegistrationController> logger)
        {
            _registrationRepository = registrationRepository;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with email and password
        /// </summary>
        /// <param name="registerUserDto">Registration information including user credentials and profile</param>
        /// <returns>Details of the newly registered user</returns>
        /// <response code="200">User registered successfully</response>
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
                var registeredUser = await _registrationRepository.Registration(registerUserDto);
                return Ok(registeredUser);
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
        /// Initiates Google OAuth registration flow
        /// </summary>
        /// <returns>Challenge result that redirects to Google authentication page</returns>
        /// <response code="302">Redirects to Google authentication page</response>
        /// <remarks>
        /// This endpoint starts the Google OAuth registration process.
        /// After authentication, the user will be redirected to the Google callback endpoint.
        /// </remarks>
        [HttpGet("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme },
                    { "returnUrl", "/api/registration/complete-registration" }
                }
            };
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        
        /// <summary>
        /// Handles the callback from Google OAuth for registration
        /// </summary>
        /// <returns>Redirect to complete registration or login page</returns>
        /// <response code="302">Redirects to complete registration form or login page</response>
        /// <remarks>
        /// This endpoint processes the Google authentication response and starts the registration process.
        /// </remarks>
        [HttpGet("google-callback")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!result.Succeeded)
                    return RedirectToAction("Error", new { error = "External authentication failed" });
                
                // Try to get id_token first, fallback to access_token
                var token = result.Properties.GetTokenValue("id_token");
                if (string.IsNullOrEmpty(token))
                {
                    token = result.Properties.GetTokenValue("access_token");
                    if (string.IsNullOrEmpty(token))
                    {
                        var availableTokens = string.Join(", ", 
                            result.Properties.GetTokens().Select(t => $"{t.Name}"));
                        return RedirectToAction("Error", new { 
                            error = $"No authentication tokens received. Available: {availableTokens}" 
                        });
                    }
                }
                
                var registrationResult = await _registrationRepository.GoogleRegister(token);
                
                if (!registrationResult)
                    return RedirectToAction("Error", new { error = "User already registered" });
                
                // Redirect directly to login page
                return Redirect("/login");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Registers a user with Google account information
        /// </summary>
        /// <param name="token">Google authentication token</param>
        /// <returns>Registration result with redirect URL</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">If registration information is invalid or user already exists</response>
        /// <response code="404">If required data is not found</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/google-registration
        ///     "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI..."
        ///     
        /// Sample response:
        ///
        ///     {
        ///         "success": true,
        ///         "message": "Registration successful",
        ///         "redirectTo": "/api/auth/google-authorization?token=eyJhbGciOiJSUzI1NiIsImtpZCI6IjI..."
        ///     }
        /// </remarks>
        [HttpPost("google-registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrationGoogle(string token)
        {
            try
            {
                // Try to register the user
                var result = await _registrationRepository.GoogleRegister(token);
                if (!result)
                    return BadRequest("User already registered");
                
                // Automatically redirect to authentication endpoint
                return Ok(new { 
                    success = true, 
                    message = "Registration successful", 
                    redirectTo = "/api/auth/google-authorization?token=" + token 
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
            return Ok(new { 
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

        /// <summary>
        /// Endpoint for completing registration with additional user details after OAuth authentication
        /// </summary>
        /// <returns>User data and required fields for completing registration</returns>
        /// <response code="200">Returns partial user data and required fields</response>
        /// <response code="400">If OAuth session data is missing</response>
        /// <remarks>
        /// This endpoint is used by the frontend to display a form for completing user registration
        /// after OAuth authentication.
        /// 
        /// Sample response:
        ///
        ///     {
        ///         "message": "Please complete your registration by providing additional information",
        ///         "userData": {
        ///             "email": "user@example.com",
        ///             "firstName": "John",
        ///             "lastName": "Smith"
        ///         },
        ///         "requiredFields": ["username"]
        ///     }
        /// </remarks>
        [HttpGet("complete-registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CompleteRegistration()
        {
            try
            {
                // Get stored OAuth data from session
                var email = HttpContext.Session.GetString("GoogleEmail");
                var firstName = HttpContext.Session.GetString("GoogleFirstName");
                var lastName = HttpContext.Session.GetString("GoogleLastName");
                
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Complete registration accessed without OAuth data");
                    return BadRequest(new {
                        error = "Missing registration data",
                        message = "Please start the registration process again"
                    });
                }
                
                // For API response, return the data we have and what's needed
                return Ok(new { 
                    message = "Please complete your registration by providing additional information",
                    userData = new {
                        email = email,
                        firstName = firstName,
                        lastName = lastName
                    },
                    requiredFields = new[] { "username" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in complete registration endpoint");
                return BadRequest(new { error = "Registration error", message = "Please try again" });
            }
        }
        
        /// <summary>
        /// Process the completed registration form with additional user details
        /// </summary>
        /// <param name="profileDto">Additional user profile information</param>
        /// <returns>Registration success confirmation and redirect</returns>
        /// <response code="200">Profile completed successfully</response>
        /// <response code="400">If profile data is invalid or session expired</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Registration/complete-profile
        ///     {
        ///         "email": "user@example.com",
        ///         "userName": "johnsmith"
        ///     }
        ///     
        /// Sample response:
        ///
        ///     {
        ///         "success": true,
        ///         "message": "Registration completed successfully",
        ///         "redirectTo": "/auth/login"
        ///     }
        /// </remarks>
        [HttpPost("complete-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto profileDto)
        {
            try
            {
                // Get stored OAuth data
                var email = HttpContext.Session.GetString("GoogleEmail");
                
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Complete profile attempt with missing session data");
                    return BadRequest(new { 
                        error = "Registration session expired",
                        message = "Please start the registration process again" 
                    });
                }
                
                // Override the email with the one from session for security
                profileDto.Email = email;
                
                // Validate required fields
                if (string.IsNullOrEmpty(profileDto.UserName))
                {
                    return BadRequest(new { error = "Missing required fields" });
                }
                
                // Update the user profile with additional information
                await _registrationRepository.CompleteGoogleRegistration(profileDto);
                
                // Clear session data
                HttpContext.Session.Remove("GoogleEmail");
                HttpContext.Session.Remove("GoogleFirstName");
                HttpContext.Session.Remove("GoogleLastName");
                
                _logger.LogInformation("User completed Google registration: {Email}", email);
                
                return Ok(new { 
                    success = true, 
                    message = "Registration completed successfully",
                    redirectTo = "/auth/login"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing profile");
                return BadRequest(new { error = "Failed to complete profile", message = ex.Message });
            }
        }
    }
    
    /// <summary>
    /// DTO for handling completion of user profile after OAuth registration
    /// </summary>
    public class CompleteProfileDto
    {
        /// <summary>
        /// Email address of the user (pre-filled from OAuth)
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Username chosen by the user
        /// </summary>
        public string UserName { get; set; }
        // Add other fields as needed
    }
}
