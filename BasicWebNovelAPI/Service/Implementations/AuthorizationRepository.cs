using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using Google.Apis.Auth;
using BasicWebNovelAPI.Enum;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly ITokenRepository _tokenRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthorizationRepository> _logger;

        public AuthorizationRepository(
            BasicWebNovelContext context, 
            ITokenRepository tokenRepository,
            IEmailRepository emailRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthorizationRepository> logger)
        {
            _context = context;
            _tokenRepository = tokenRepository;
            _emailRepository = emailRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> LogIn(GetLoginDto getLoginDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == getLoginDto.Email);

            if (user == null)
                throw new Exception("User not found");

            bool isCorrectPassword = getLoginDto.Password.PasswordVerify(user.PasswordHash);

            if (user.LockoutExpirationTime.HasValue && user.LockoutExpirationTime > DateTime.Now)
            {
                throw new Exception("Your account is locked. Please try again after an hour.");
            }

            if (!isCorrectPassword) 
            {
                user.FailedLoginAttempts++;
                
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutExpirationTime = DateTime.Now.AddHours(1);
                    user.FailedLoginAttempts = 0; 
                    await _context.SaveChangesAsync();
                    throw new Exception("Too many failed login attempts. Your account is locked for an hour.");
                }

                await _context.SaveChangesAsync();
                throw new Exception($"Incorrect password. You have {5 - user.FailedLoginAttempts} attempts left.");
            }

            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            var roles = new List<string> { user.Role.RoleName };
            string token = _tokenRepository.GenerateToken(user, roles);

            return token;
        }

        public async Task<string> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == verifyCodeDto.Email);

            if (user == null)
                return "User Not Found";

            bool isCorrectPassword = verifyCodeDto.Password.PasswordVerify(user.PasswordHash);

            if (!isCorrectPassword)
                return "Password is incorrect";

            if (user.TemporaryCode != verifyCodeDto.TemporaryCode || DateTime.Now > user.CodeExpirationTime)
                return "Invalid or expired code";

            var roles = new List<string> { user.Role.RoleName };
            string token = _tokenRepository.GenerateToken(user, roles);

            user.TemporaryCode = null;
            user.CodeExpirationTime = null;
            await _context.SaveChangesAsync();

            return token;
        }

        

        public async Task<User> GetGraphData(string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    "https://graph.facebook.com/v16.0/me?fields=id,name,email&access_token=" + accessToken);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Facebook API error: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                var facebookData = JsonSerializer.Deserialize<FacebookGraphData>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (facebookData == null || string.IsNullOrEmpty(facebookData.Email))
                    throw new Exception("Invalid Facebook data or email is missing");

                var user = new User { 
                    Email = facebookData.Email,
                    FirstName = facebookData.Name?.Split(' ').FirstOrDefault() ?? string.Empty,
                    LastName = facebookData.Name?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty
                };
                
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get Facebook data: {ex.Message}");
            }
        }

        public async Task<string> FaceBookAuthorization(string accessToken)
        {
            try
            {
                var userFromFacebook = await GetGraphData(accessToken);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == userFromFacebook.Email);

                if (user == null)
                    throw new Exception("No user is registered with this email");

                var roles = new List<string> { user.Role.RoleName };
                string token = _tokenRepository.GenerateToken(user, roles);

                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Facebook authorization failed: {ex.Message}");
            }
        }

        public async Task<User> GetGoogleUserData(string token)
        {
            try
            {
                _logger.LogInformation("Getting Google user data from token");
                
                // Debug helper - show token structure
                var tokenParts = token.Split('.');
                _logger.LogDebug("Token format: {PartCount} parts, first 10 chars: {Start}...", 
                    tokenParts.Length, token.Substring(0, Math.Min(10, token.Length)));
                    
                string email = null;
                string firstName = null;
                string lastName = null;
                string picture = null;
                bool useAccessTokenApproach = false;
                
                // Check token from config
                _logger.LogDebug("Client ID from config: {ClientId}", 
                    _configuration["Authentication:Google:ClientId"] ?? "NULL");
                    
                // Check if this is an ID token or access token based on format
                if (token.Split('.').Length == 3 && !useAccessTokenApproach) // Typical JWT ID token format has 3 parts
                {
                    _logger.LogInformation("Processing token as ID token");
                    
                    // This is an ID token, validate it
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                    };
                    
                    // Debug - log audience
                    foreach (var aud in settings.Audience)
                    {
                        _logger.LogDebug("Using audience for validation: {Audience}", aud);
                    }
                    
                    _logger.LogInformation("Validating Google ID token with client ID: {ClientId}", 
                        _configuration["Authentication:Google:ClientId"]);
                        
                    try
                    {
                        var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                        
                        if (payload == null)
                        {
                            _logger.LogWarning("Google payload is null after validation");
                            throw new Exception("Invalid Google data - payload is null");
                        }
                        
                        // Debug - log payload info
                        _logger.LogDebug("Payload info - Subject: {Subject}, Audience: {Audience}, Issuer: {Issuer}",
                            payload.Subject, payload.Audience, payload.Issuer);
                        
                        if (string.IsNullOrEmpty(payload.Email))
                        {
                            _logger.LogWarning("Google payload does not contain email");
                            throw new Exception("Invalid Google data or email is missing");
                        }
                        
                        _logger.LogInformation("Successfully extracted data from ID token for email: {Email}", payload.Email);
                        
                        email = payload.Email;
                        firstName = payload.GivenName;
                        lastName = payload.FamilyName;
                        picture = payload.Picture;
                        
                        // Log if name is missing
                        if (string.IsNullOrEmpty(firstName))
                        {
                            _logger.LogWarning("FirstName is missing in Google payload for email: {Email}", email);
                        }
                        
                        if (string.IsNullOrEmpty(lastName))
                        {
                            _logger.LogWarning("LastName is missing in Google payload for email: {Email}", email);
                        }
                    }
                    catch (Exception valEx)
                    {
                        // Special debug for validation errors
                        _logger.LogError(valEx, "ID token validation error: {Message}", valEx.Message);
                        
                        // Try access token approach as fallback
                        _logger.LogInformation("ID token validation failed, trying as access token instead");
                        useAccessTokenApproach = true;
                    }
                }
                else
                {
                    useAccessTokenApproach = true;
                }
                
                if (useAccessTokenApproach)
                {
                    _logger.LogInformation("Processing token as access token");
                    
                    // This is likely an access token, use it to fetch user info
                    var client = _httpClientFactory.CreateClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, 
                        "https://www.googleapis.com/oauth2/v3/userinfo");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    _logger.LogInformation("Calling Google userinfo endpoint with access token");
                    
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Google API returned error: {StatusCode}, Content: {Content}", 
                            response.StatusCode, content);
                            
                        throw new Exception($"Google API error: {response.StatusCode}. Response: {content}");
                    }
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Google API response: {Response}", responseContent);
                    
                    var userData = JsonSerializer.Deserialize<GoogleUserData>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (userData == null)
                    {
                        _logger.LogWarning("Failed to deserialize Google user data");
                        throw new Exception("Invalid Google data - failed to deserialize response");
                    }
                    
                    if (string.IsNullOrEmpty(userData.Email))
                    {
                        _logger.LogWarning("Google user data does not contain email");
                        throw new Exception("Invalid Google data or email is missing");
                    }
                    
                    _logger.LogInformation("Successfully extracted data from access token for email: {Email}", userData.Email);
                    
                    email = userData.Email;
                    
                    // Instead of direct assignments, extract name more intelligently
                    if (!string.IsNullOrWhiteSpace(userData.GivenName)) {
                        firstName = userData.GivenName;
                    } else if (!string.IsNullOrWhiteSpace(userData.Name)) {
                        // If we have a full name but no given_name, try to split it
                        var nameParts = userData.Name.Split(' ');
                        if (nameParts.Length > 0) {
                            firstName = nameParts[0];
                        }
                    }
                    
                    if (!string.IsNullOrWhiteSpace(userData.FamilyName)) {
                        lastName = userData.FamilyName;
                    } else if (!string.IsNullOrWhiteSpace(userData.Name)) {
                        // If we have a full name but no family_name, try to extract last name
                        var nameParts = userData.Name.Split(' ');
                        if (nameParts.Length > 1) {
                            lastName = string.Join(" ", nameParts.Skip(1));
                        }
                    }
                    
                    picture = userData.Picture;
                    
                    // Log detailed name information
                    _logger.LogInformation("Name data from API: GivenName={GivenName}, FamilyName={FamilyName}, Name={Name}, ExtractedFirstName={ExtractedFirstName}, ExtractedLastName={ExtractedLastName}", 
                        userData.GivenName, userData.FamilyName, userData.Name, firstName, lastName);
                    
                    // Log if name is missing
                    if (string.IsNullOrEmpty(firstName))
                    {
                        _logger.LogWarning("FirstName is missing in Google userinfo for email: {Email}", email);
                    }
                    
                    if (string.IsNullOrEmpty(lastName))
                    {
                        _logger.LogWarning("LastName is missing in Google userinfo for email: {Email}", email);
                    }
                }
                
                // Set default values for required fields if they're still null
                if (string.IsNullOrWhiteSpace(firstName))
                    firstName = "Google";

                if (string.IsNullOrWhiteSpace(lastName))
                    lastName = "User";
                
                var user = new User
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    AuthIssuer = AuthIssuer.GOOGLE
                };
                
                _logger.LogInformation("Successfully created user data object from Google data for {Email} with FirstName: {FirstName}, LastName: {LastName}", 
                    email, firstName, lastName);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Google user data: {ErrorType} - {ErrorMessage}", 
                    ex.GetType().Name, ex.Message);
                    
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerType} - {InnerMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                throw new Exception($"Failed to get Google user data: {ex.Message}");
            }
        }

        public async Task<string> GoogleAuthorization(string token)
        {
            try
            {
                _logger.LogInformation("Starting Google authorization process");
                
                // Get user data from Google
                var googleUserData = await GetGoogleUserData(token);
                
                _logger.LogInformation("Checking if user with email {Email} exists", googleUserData.Email);
                
                // Check if user exists
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == googleUserData.Email);
                
                // If user doesn't exist, return a special response to trigger registration flow
                if (user == null)
                {
                    _logger.LogInformation("Google authorization - User not found, needs registration: {Email}", googleUserData.Email);
                    
                    // Return empty string as a signal that registration is needed
                    // The controller will handle this case specially
                    return string.Empty;
                }
                
                _logger.LogInformation("User found, generating JWT token for user: {Email}", user.Email);
                
                // Ensure user has a valid role
                if (user.Role == null)
                {
                    _logger.LogWarning("User {Email} has no role assigned. Assigning default role.", user.Email);
                    
                    // Get the default role (changed from 2 to 1 for regular users)
                    var defaultRole = await _context.Roles.FindAsync(1);
                    
                    if (defaultRole != null)
                    {
                        user.RoleId = defaultRole.Id;
                        user.Role = defaultRole;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError("Default role not found. Cannot assign role to user {Email}", user.Email);
                        throw new Exception("User role configuration error");
                    }
                }
                
                // User exists, generate JWT token
                var roles = new List<string> { user.Role.RoleName };
                
                // For debugging, log the role being assigned
                _logger.LogInformation("Generating token with role: {Role}", string.Join(", ", roles));
                
                string jwtToken = _tokenRepository.GenerateToken(user, roles);
                
                // Validate token format
                if (string.IsNullOrEmpty(jwtToken) || !jwtToken.Contains("."))
                {
                    _logger.LogError("Generated token appears invalid: {Token}", 
                        string.IsNullOrEmpty(jwtToken) ? "NULL" : jwtToken.Substring(0, Math.Min(10, jwtToken.Length)) + "...");
                    throw new Exception("Token generation failed");
                }
                
                _logger.LogInformation("Successfully generated JWT token for Google user");
                
                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authorization failed: {ErrorType} - {ErrorMessage}", 
                    ex.GetType().Name, ex.Message);
                    
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerType} - {InnerMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                throw new Exception($"Google authorization failed: {ex.Message}");
            }
        }
    }

    public class FacebookGraphData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class GoogleUserData
    {
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }
}
