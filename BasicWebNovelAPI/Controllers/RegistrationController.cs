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
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpPost("register")]
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

        [HttpGet("google-login")]
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
        
        [HttpGet("google-callback")]
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
        
        [HttpPost("google-registration")]
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

        [HttpPost("facebook-registration")]
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

        [HttpGet("error")]
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

        [HttpGet("success")]
        public IActionResult Success()
        {
            return Ok(new { success = true, message = "Registration successful" });
        }

        /// <summary>
        /// Endpoint for completing registration with additional user details after OAuth authentication
        /// </summary>
        [HttpGet("complete-registration")]
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
        [HttpPost("complete-profile")]
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
        public string Email { get; set; }
        public string UserName { get; set; }
        // Add other fields as needed
    }
}
