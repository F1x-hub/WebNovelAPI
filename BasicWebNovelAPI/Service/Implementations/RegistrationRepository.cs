using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
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
        private readonly IEmailRepository _emailRepository;

        public RegistrationRepository(
            BasicWebNovelContext context, 
            IMapper mapper, 
            IAuthorizationRepository authorizationRepository,
            IConfiguration configuration,
            ILogger<RegistrationRepository> logger,
            IImageRepository imageRepository,
            IHttpClientFactory httpClientFactory,
            IEmailRepository emailRepository)
        {
            _context = context;
            _mapper = mapper;
            _authorizationRepository = authorizationRepository;
            _configuration = configuration;
            _logger = logger;
            _imageRepository = imageRepository;
            _httpClientFactory = httpClientFactory;
            _emailRepository = emailRepository;
        }

        public async Task<GetUserDto> Registration(RegisterUserDto registerUserDto)
        {
            // Check if user with the same email exists
            var emailExists = await _context.Users.AnyAsync(u => u.Email == registerUserDto.Email);
            if (emailExists)
                throw new Exception("User with this email already exists!");

            // Check if user with the same username exists
            var usernameExists = await _context.Users.AnyAsync(u => u.UserName == registerUserDto.UserName);
            if (usernameExists)
                throw new Exception("Username is already taken. Please choose a different username.");

            var newUser = _mapper.Map<User>(registerUserDto);
            newUser.RoleId = registerUserDto.RoleId;
            newUser.PasswordHash = registerUserDto.Password.PasswordHash();
            newUser.AuthIssuer = AuthIssuer.JWT;
            
            // Generate verification code
            Random random = new Random();
            string verificationCode = random.Next(100000, 999999).ToString();
            newUser.TemporaryCode = verificationCode;
            newUser.CodeExpirationTime = DateTime.Now.AddHours(24);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Send verification email
            string emailBody = $"Your verification code is: {verificationCode}\n\nThis code will expire in 24 hours. Please verify your email to activate your account.";
            await _emailRepository.SendToEmail(newUser.Email, emailBody);

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

                // Check if user with the same email exists
                var emailExists = await _context.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
                if (emailExists != null)
                    return false;

                string proposedUsername = userData.Email.Split('@')[0];
                
                // Check if username exists
                var usernameExists = await _context.Users.AnyAsync(u => u.UserName == proposedUsername);
                if (usernameExists)
                {
                    // Append random number to make username unique
                    Random random = new Random();
                    proposedUsername = $"{proposedUsername}{random.Next(1000, 9999)}";
                }

                // Get default role for new users (assuming role ID 2 is for regular users)
                var defaultRoleId = 2;
                
                var user = new User
                {
                    Email = userData.Email,
                    UserName = proposedUsername,
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

        public async Task<bool> VerifyEmail(VerifyCodeDto verifyCodeDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyCodeDto.Email);
            
            if (user == null)
                throw new Exception("User not found");
                
            bool isCorrectPassword = verifyCodeDto.Password.PasswordVerify(user.PasswordHash);
            
            if (!isCorrectPassword)
                throw new Exception("Password is incorrect");
                
            if (user.TemporaryCode != verifyCodeDto.TemporaryCode)
                throw new Exception("Invalid verification code");
                
            if (!user.CodeExpirationTime.HasValue || DateTime.Now > user.CodeExpirationTime.Value)
                throw new Exception("Verification code has expired. Please request a new code");
                
            // Clear the temporary code and expiration time after successful verification
            user.TemporaryCode = null;
            user.CodeExpirationTime = null;
            
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
