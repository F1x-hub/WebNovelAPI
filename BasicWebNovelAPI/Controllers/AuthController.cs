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
        /// Initiates Google authentication flow
        /// </summary>
        /// <param name="returnUrl">URL to redirect after successful authentication (default: "/")</param>
        /// <returns>Challenge result that redirects to Google authentication page</returns>
        /// <response code="302">Redirects to Google authentication page</response>
        [HttpGet("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            _logger.LogInformation("Starting Google login flow for return URL: {ReturnUrl}", returnUrl);
            
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme },
                    { "returnUrl", returnUrl }
                }
            };
            
            // Store returnUrl in session as backup in case correlation fails
            HttpContext.Session.SetString("GoogleReturnUrl", returnUrl);
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        
        /// <summary>
        /// Handles the callback from Google OAuth authentication
        /// </summary>
        /// <returns>Redirect to frontend with authentication token or error page</returns>
        /// <response code="302">Redirects to specified return URL with token</response>
        /// <response code="200">Error response if authentication fails</response>
        [HttpGet("google-callback")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                _logger.LogInformation("Google callback initiated");
                
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Google authentication did not succeed. Details: {FailureMessage}", 
                        result.Failure?.Message ?? "No failure details available");
                        
                    // Check for correlation failure specifically
                    if (result.Failure?.Message?.Contains("Correlation failed") == true)
                    {
                        _logger.LogWarning("Correlation failure detected - attempting fallback method");
                        
                        // Try to extract token directly from query parameters
                        var code = HttpContext.Request.Query["code"].ToString();
                        if (!string.IsNullOrEmpty(code))
                        {
                            _logger.LogInformation("Found code in query parameters, exchanging for token");
                            
                            // At this point we can direct user to registration completion or login
                            // Get cached return URL from session
                            var cachedReturnUrl = HttpContext.Session.GetString("GoogleReturnUrl") ?? "/";
                            _logger.LogInformation("Retrieved return URL from session: {ReturnUrl}", cachedReturnUrl);
                            
                            return Redirect("/api/registration/complete-registration");
                        }
                    }
                    
                    return RedirectToAction("Error", new { error = "External authentication failed: " + result.Failure?.Message });
                }
                
                _logger.LogInformation("Google authentication succeeded, attempting to get token");
                
                // Get token - try id_token first, then access_token as fallback
                var token = result.Properties.GetTokenValue("id_token");
                if (string.IsNullOrEmpty(token))
                {
                    token = result.Properties.GetTokenValue("access_token");
                    if (string.IsNullOrEmpty(token))
                    {
                        var availableTokens = string.Join(", ", 
                            result.Properties.GetTokens().Select(t => $"{t.Name}"));
                        
                        _logger.LogWarning("No auth tokens received. Available tokens: {Tokens}", availableTokens);
                        return RedirectToAction("Error", new { 
                            error = $"No authentication tokens received. Available: {availableTokens}" 
                        });
                    }
                    
                    _logger.LogInformation("Using access_token for authentication");
                }
                else
                {
                    _logger.LogInformation("Using id_token for authentication");
                }
                
                // Try to authenticate the user
                _logger.LogInformation("Attempting to authorize with Google token");
                var jwtToken = await _authorizationRepository.GoogleAuthorization(token);
                
                // If token is empty, it means user doesn't exist and needs registration
                if (string.IsNullOrEmpty(jwtToken))
                {
                    _logger.LogInformation("Google callback - User not found, initiating registration");
                    
                    // Get the user's Google data
                    _logger.LogInformation("Fetching Google user data");
                    var googleUserData = await _authorizationRepository.GetGoogleUserData(token);
                    
                    _logger.LogInformation("Storing Google user data for email: {Email}", googleUserData.Email);
                    
                    // Begin registration process automatically
                    _logger.LogInformation("Starting Google registration process");
                    var registered = await _registrationRepository.GoogleRegister(token);
                    
                    if (registered)
                    {
                        // After successful registration, try to authenticate
                        jwtToken = await _authorizationRepository.GoogleAuthorization(token);
                        
                        if (!string.IsNullOrEmpty(jwtToken))
                        {
                            // Registration and authentication successful
                            _logger.LogInformation("Google registration and authentication successful, redirecting with token");
                            var redirectUrl = result.Properties.Items["returnUrl"] ?? "/";
                            
                            // Ensure redirectUrl has proper protocol
                            if (!redirectUrl.StartsWith("http://") && !redirectUrl.StartsWith("https://"))
                            {
                                // Get origin from request
                                var origin = $"{Request.Scheme}://{Request.Host}";
                                redirectUrl = $"{origin}{redirectUrl}";
                            }
                            
                            // Use a dedicated endpoint for successful login from the frontend perspective
                            return Redirect($"{redirectUrl}?token={jwtToken}");
                        }
                    }
                    
                    // If auto-login fails, redirect to login page
                    _logger.LogInformation("Redirecting to login page");
                    return Redirect("/login");
                }
                
                // Normal login succeeded
                _logger.LogInformation("Google authentication successful, redirecting with token");
                var returnUrl = result.Properties.Items["returnUrl"] ?? "/";
                
                // Ensure returnUrl has proper protocol
                if (!returnUrl.StartsWith("http://") && !returnUrl.StartsWith("https://"))
                {
                    // Get origin from request
                    var origin = $"{Request.Scheme}://{Request.Host}";
                    returnUrl = $"{origin}{returnUrl}";
                }
                
                return Redirect($"{returnUrl}?token={jwtToken}");
            }
            catch (Exception ex)
            {
                // Log full exception details including stack trace for debugging
                _logger.LogError(ex, "Detailed error in Google callback: {ErrorType} - {ErrorMessage}", 
                    ex.GetType().Name, ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerType} - {InnerMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                // Return a detailed error message directly (for debugging only)
                return Ok(new {
                    error = true,
                    debug = true,
                    errorType = ex.GetType().Name,
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message,
                    source = ex.Source,
                    stackTrace = ex.StackTrace?.Split('\n')
                });
            }
        }
        
        /// <summary>
        /// Authorizes a user with Google token
        /// </summary>
        /// <param name="token">Google authentication token</param>
        /// <returns>JWT token for authenticated user</returns>
        /// <response code="200">Returns JWT token for the user</response>
        /// <response code="400">If token is invalid or authentication fails</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/google-authorization
        ///     "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI..."
        ///     
        /// Sample response:
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///     }
        /// </remarks>
        [HttpPost("google-authorization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AuthorizationGoogle(string token)
        {
            try
            {
                var jwtToken = await _authorizationRepository.GoogleAuthorization(token);
                
                // If token is empty, it means user doesn't exist and needs registration
                if (string.IsNullOrEmpty(jwtToken))
                {
                    // Begin registration process automatically
                    var registered = await _registrationRepository.GoogleRegister(token);
                    
                    if (registered)
                    {
                        // Try to authenticate again after registration
                        jwtToken = await _authorizationRepository.GoogleAuthorization(token);
                        
                        if (!string.IsNullOrEmpty(jwtToken))
                        {
                            // Return the token if registration and authentication successful
                            return Ok(new { token = jwtToken });
                        }
                    }
                    
                    // Something went wrong with registration or authentication
                    return BadRequest("Registration or authentication failed");
                }
                
                return Ok(new { token = jwtToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google authorization");
                return BadRequest(new { error = "Authentication failed", message = "Please try again later" });
            }
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
