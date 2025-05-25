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
    }
}
