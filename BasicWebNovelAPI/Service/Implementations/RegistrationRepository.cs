using AutoMapper;
using BasicWebNovelAPI.Controllers;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationRepository> _logger;
        private readonly IImageRepository _imageRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrationRepository(
            BasicWebNovelContext context, 
            IMapper mapper, 
            IAuthorizationRepository authorizationRepository,
            IConfiguration configuration,
            ILogger<RegistrationRepository> logger,
            IImageRepository imageRepository,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mapper = mapper;
            _authorizationRepository = authorizationRepository;
            _configuration = configuration;
            _logger = logger;
            _imageRepository = imageRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetUserDto> Registration(RegisterUserDto registerUserDto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Email == registerUserDto.Email);
            if (userExists)
                throw new Exception("User already exists!");

            var newUser = _mapper.Map<User>(registerUserDto);
            newUser.RoleId = registerUserDto.RoleId;
            newUser.PasswordHash = registerUserDto.Password.PasswordHash();
            newUser.AuthIssuer = AuthIssuer.JWT;

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var userDto = _mapper.Map<GetUserDto>(newUser);
            return userDto;
        }

        public async Task<bool> FaceBookRegister(string accessToken)
        {
            try
            {
                var userData = await _authorizationRepository.GetGraphData(accessToken);
                if (userData == null || string.IsNullOrEmpty(userData.Email))
                    throw new Exception("Invalid Facebook data or email is missing");

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
                if (existingUser != null)
                    return false;

                // Get default role for new users (assuming role ID 2 is for regular users)
                var defaultRoleId = 2;
                
                var user = new User
                {
                    Email = userData.Email,
                    UserName = userData.Email.Split('@')[0],
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    AuthIssuer = AuthIssuer.FACEBOOK,
                    PasswordHash = "FacebookAuth".PasswordHash(),
                    RoleId = defaultRoleId
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Facebook registration failed: {ex.Message}");
            }
        }

        public async Task<bool> GoogleRegister(string token)
        {
            try
            {
                _logger.LogInformation("Starting Google registration process");
                
                string email = null;
                string firstName = null;
                string lastName = null;
                string picture = null;
                
                // Try to extract user data from ID token first
                if (token.Split('.').Length == 3) // Typical JWT ID token format has 3 parts
                {
                    try
                    {
                        _logger.LogInformation("Processing token as ID token");
                        
                        var settings = new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                        };
                        
                        var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                        
                        if (payload != null)
                        {
                            _logger.LogInformation("Successfully validated ID token for: {Email}", payload.Email);
                            
                            email = payload.Email;
                            
                            // Create a GoogleUserInfo object to use our helper methods
                            var userInfo = new GoogleUserInfo
                            {
                                Email = payload.Email,
                                GivenName = payload.GivenName,
                                FamilyName = payload.FamilyName,
                                Name = payload.Name,
                                Picture = payload.Picture
                            };
                            
                            // Use the helper methods for consistent name extraction
                            var extractedFirstName = userInfo.GetFirstName();
                            var extractedLastName = userInfo.GetLastName();
                            
                            if (!string.IsNullOrWhiteSpace(extractedFirstName))
                                firstName = extractedFirstName;
                                
                            if (!string.IsNullOrWhiteSpace(extractedLastName))
                                lastName = extractedLastName;
                                
                            picture = payload.Picture;
                            
                            _logger.LogInformation("Extracted profile data from ID token: GivenName={GivenName}, FamilyName={FamilyName}, Name={Name}", 
                                payload.GivenName, payload.FamilyName, payload.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to validate ID token, will try as access token");
                    }
                }
                
                // If ID token approach failed or didn't yield names, use access token approach
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    _logger.LogInformation("Using access token to fetch user profile");
                    
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                    
                    // Try Google userinfo endpoint first
                    try
                    {
                        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation("Google userinfo response: {Response}", content);
                            
                            var userData = JsonSerializer.Deserialize<GoogleUserInfo>(content, 
                                new JsonSerializerOptions { 
                                    PropertyNameCaseInsensitive = true,
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                });
                            
                            if (userData != null)
                            {
                                email = userData.Email ?? email;
                                
                                // Use the helper methods to extract names
                                var extractedFirstName = userData.GetFirstName();
                                var extractedLastName = userData.GetLastName();
                                
                                if (!string.IsNullOrWhiteSpace(extractedFirstName))
                                    firstName = extractedFirstName;
                                    
                                if (!string.IsNullOrWhiteSpace(extractedLastName))
                                    lastName = extractedLastName;
                                
                                picture = userData.Picture ?? picture;
                                
                                _logger.LogInformation("Extracted profile data from userinfo: GivenName={GivenName}, FamilyName={FamilyName}, Name={Name}", 
                                    userData.GivenName, userData.FamilyName, userData.Name);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Google userinfo API error: {StatusCode}", response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error calling Google userinfo endpoint");
                    }
                    
                    // If we still don't have name info, try the People API as a last resort
                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                    {
                        try
                        {
                            var peopleResponse = await httpClient.GetAsync(
                                "https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses");
                            
                            if (peopleResponse.IsSuccessStatusCode)
                            {
                                var content = await peopleResponse.Content.ReadAsStringAsync();
                                _logger.LogDebug("Google People API response: {Response}", content);
                                
                                var peopleData = JsonSerializer.Deserialize<GooglePeopleData>(content,
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                
                                if (peopleData?.Names?.Count > 0)
                                {
                                    // Take the first name entry that's marked as primary if possible
                                    var primaryName = peopleData.Names.FirstOrDefault(n => n.Metadata?.Primary == true) 
                                        ?? peopleData.Names.First();
                                    
                                    firstName = primaryName.GivenName ?? firstName;
                                    lastName = primaryName.FamilyName ?? lastName;
                                    
                                    _logger.LogInformation("Extracted profile data from People API: GivenName={GivenName}, FamilyName={FamilyName}", 
                                        firstName, lastName);
                                }
                                
                                // If email is still null, try to get it from People API
                                if (string.IsNullOrEmpty(email) && peopleData?.EmailAddresses?.Count > 0)
                                {
                                    var primaryEmail = peopleData.EmailAddresses.FirstOrDefault(e => e.Metadata?.Primary == true)
                                        ?? peopleData.EmailAddresses.First();
                                    
                                    email = primaryEmail.Value;
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Google People API error: {StatusCode}", peopleResponse.StatusCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error calling Google People API");
                        }
                    }
                }
                
                // Double-check required fields before creating user
                if (string.IsNullOrEmpty(email))
                {
                    throw new Exception("Email is required for registration");
                }
                
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                    return false;
                
                // Get default role for new users (usually regular user role)
                var defaultRoleId = 1; // Regular user role (changed from 2 to 1)
                
                // Verify the role exists
                var role = await _context.Roles.FindAsync(defaultRoleId);
                if (role == null)
                {
                    _logger.LogError("Default role with ID {RoleId} not found", defaultRoleId);
                    throw new Exception("User role configuration error - default role not found");
                }
                
                // Do not set default values for firstName and lastName
                // We'll just use whatever we got from Google, or empty strings
                
                _logger.LogInformation("Creating new user with Email: {Email}, FirstName: {FirstName}, LastName: {LastName}, Role: {Role}", 
                    email, firstName ?? "", lastName ?? "", role.RoleName);
                
                // Create new user - ensure all non-nullable fields have values
                var user = new User
                {
                    Email = email,
                    UserName = GenerateUniqueUsername(email, firstName, lastName),
                    FirstName = firstName ?? "", // Use empty string instead of default
                    LastName = lastName ?? "",   // Use empty string instead of default
                    AuthIssuer = AuthIssuer.GOOGLE,
                    PasswordHash = "GoogleAuth".PasswordHash(),
                    RoleId = role.Id,
                    Role = role
                };
                
                _logger.LogInformation("FINAL NAME VALUES: FirstName='{FirstName}', LastName='{LastName}'", 
                    user.FirstName, user.LastName);
                
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User {Email} successfully registered with role {Role}", user.Email, role.RoleName);
                
                // If profile picture URL is available, save it to AWS S3
                if (!string.IsNullOrWhiteSpace(picture))
                {
                    try
                    {
                        _logger.LogInformation("Downloading profile picture from Google: {PictureUrl}", picture);
                        
                        // Download the image from Google
                        var client = _httpClientFactory.CreateClient();
                        var imageResponse = await client.GetAsync(picture);
                        
                        if (imageResponse.IsSuccessStatusCode)
                        {
                            // Read image content
                            var imageContent = await imageResponse.Content.ReadAsByteArrayAsync();
                            
                            // Create a temporary file
                            var fileName = $"google_profile_{Guid.NewGuid()}.jpg";
                            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                            
                            // Save image to temporary file
                            await System.IO.File.WriteAllBytesAsync(tempFilePath, imageContent);
                            
                            // Create IFormFile from the temporary file
                            using (var fileStream = new FileStream(tempFilePath, FileMode.Open))
                            {
                                var formFile = new CustomFormFile(
                                    fileStream,
                                    0,
                                    fileStream.Length,
                                    "profilePicture",
                                    fileName)
                                {
                                    ContentType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg"
                                };
                                
                                // Save image to AWS S3
                                await _imageRepository.AddUserImagesAsync(user.Id, formFile);
                                
                                _logger.LogInformation("Successfully saved Google profile picture to AWS S3 for user: {UserId}", user.Id);
                            }
                            
                            // Clean up temporary file
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Delete(tempFilePath);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to download Google profile picture: {StatusCode}", imageResponse.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail registration if image saving fails
                        _logger.LogError(ex, "Error saving Google profile picture for user {UserId}: {Error}", user.Id, ex.Message);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete Google registration: {Error}", ex.Message);
                throw new Exception($"Google registration failed: {ex.Message}");
            }
        }

        private string GenerateUniqueUsername(string email, string firstName, string lastName)
        {
            // Generate a base username from email or name
            string baseUsername = !string.IsNullOrEmpty(email) 
                ? email.Split('@')[0] 
                : $"{firstName}{lastName}".ToLower();
                
            // Check if username already exists
            bool usernameExists = _context.Users.Any(u => u.UserName == baseUsername);
            
            if (!usernameExists)
                return baseUsername;
                
            // If username exists, append random digits until unique
            Random rand = new Random();
            string candidateUsername;
            
            do
            {
                candidateUsername = $"{baseUsername}{rand.Next(1000, 10000)}";
                usernameExists = _context.Users.Any(u => u.UserName == candidateUsername);
            } while (usernameExists);
            
            return candidateUsername;
        }

        public async Task<bool> CompleteGoogleRegistration(CompleteProfileDto profileDto)
        {
            try
            {
                // Find the user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == profileDto.Email);
                
                if (user == null)
                {
                    _logger.LogWarning("Attempted to complete registration for non-existent user: {Email}", profileDto.Email);
                    throw new Exception("User not found. Please try registering again.");
                }
                
                // Update user with additional information
                user.UserName = profileDto.UserName;
                
                // Add any other fields as needed
                
                _logger.LogInformation("Completing Google registration for user: {Email}", profileDto.Email);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete Google registration for {Email}", profileDto.Email);
                throw new Exception($"Failed to complete registration: {ex.Message}");
            }
        }
    }

    public class GoogleUserInfo
    {
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        
        // Helper method to get first name from combined fields
        public string GetFirstName()
        {
            if (!string.IsNullOrWhiteSpace(GivenName))
                return GivenName;
            
            if (!string.IsNullOrWhiteSpace(Name) && Name.Contains(" "))
                return Name.Split(' ')[0];
            
            if (!string.IsNullOrWhiteSpace(Name))
                return Name;
            
            return "";
        }
        
        // Helper method to get last name from combined fields
        public string GetLastName()
        {
            if (!string.IsNullOrWhiteSpace(FamilyName))
                return FamilyName;
            
            if (!string.IsNullOrWhiteSpace(Name) && Name.Contains(" "))
            {
                var parts = Name.Split(' ');
                if (parts.Length > 1)
                    return string.Join(" ", parts.Skip(1));
            }
            
            return "";
        }
    }
    
    public class GooglePeopleData
    {
        public List<GoogleNameInfo> Names { get; set; }
        public List<GoogleEmailInfo> EmailAddresses { get; set; }
    }
    
    public class GoogleNameInfo
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public GoogleMetadata Metadata { get; set; }
    }
    
    public class GoogleEmailInfo
    {
        public string Value { get; set; }
        public GoogleMetadata Metadata { get; set; }
    }
    
    public class GoogleMetadata
    {
        public bool Primary { get; set; }
        public bool Verified { get; set; }
        public string Source { get; set; }
    }

    public class CustomFormFile : IFormFile
    {
        private readonly Stream _stream;
        private readonly long _length;
        private readonly string _name;
        private readonly string _fileName;

        public CustomFormFile(Stream stream, long offset, long length, string name, string fileName)
        {
            _stream = stream;
            _length = length;
            _name = name;
            _fileName = fileName;
        }

        public string ContentType { get; set; }
        public string ContentDisposition => $"form-data; name=\"{_name}\"; filename=\"{_fileName}\"";
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public long Length => _length;
        public string Name => _name;
        public string FileName => _fileName;

        public void CopyTo(Stream target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            _stream.Position = 0;
            _stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            _stream.Position = 0;
            await _stream.CopyToAsync(target, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            _stream.Position = 0;
            return _stream;
        }
    }
}
