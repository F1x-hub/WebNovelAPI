using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
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

                var user = new User
                {
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





        public class FacebookGraphData
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

    }
}
