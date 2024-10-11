using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model;
using DotNetOpenAuth.AspNet.Clients;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using Google.Apis.Auth;
using BasicWebNovelAPI.Enum;
using System.Net;
using System.Text.Json;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IImageRepository _imageRepository;

        public AuthorizationRepository(BasicWebNovelContext context, 
                                       IMapper mapper, 
                                       ITokenRepository tokenRepository, 
                                       IImageRepository imageRepository,
                                       IEmailRepository emailRepository)
        {
            _context = context;
            _mapper = mapper;
            _tokenRepository = tokenRepository;
            _imageRepository = imageRepository;
            _emailRepository = emailRepository;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }


        public async Task<User> GetUserId(int userId)
        {
            return await _context.Users
                         .FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task<bool> UpdateUser(User updatedUser)
        {
            _context.Users.Update(updatedUser);
            await _context.SaveChangesAsync();


            return true;
        }


        public async Task<User> DeleteUserId(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }


        public async Task<GetUserDto> Registration(RegisterUserDto registerUserDto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Email == registerUserDto.Email);
            if (userExists)
                throw new Exception("User already exists!");

            var newUser = _mapper.Map<User>(registerUserDto);

            newUser.RoleId = registerUserDto.RoleId;


            newUser.PasswordHash = registerUserDto.Password.PasswordHash();

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            
            var userDto = _mapper.Map<GetUserDto>(newUser);
            return userDto;
        }

        public async Task AddUserImagesAsync(int userId, IFormFile? imageFiles)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new Exception("User Not Found");
            }

            if (imageFiles != null)
            {
                var userImage = new UserImages
                {
                    UserId = user.Id,
                    ImageSource = await _imageRepository.GenerateUserImageSource(imageFiles)
                };
                await _imageRepository.SaveUserImageInDatabase(userImage);
                
            }
              
           
        }


        public async Task<string> LogIn(GetLoginDto getLoginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == getLoginDto.Email);

            if (user == null)
                throw new Exception("User not found");

            
            var temporaryCode = new Random().Next(100000,999999).ToString();
            user.TemporaryCode = temporaryCode;
            user.CodeExpirationTime = DateTime.Now.AddMinutes(10);

            await _context.SaveChangesAsync();

            var resultEmail = await _emailRepository.SendToEmail("iraklilagvilava975@gmail.com", $"Your login code is: {temporaryCode}");

            return "Temporary code sent to your email. Please use it to complete the login.";

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

        public async Task<GoogleJsonWebSignature.Payload> GetPayLoad(string token)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, 
                new GoogleJsonWebSignature.ValidationSettings());


            return payload;
        }

        public async Task<bool> GoogleRegister(string accessToken)
        {
            var payload = await GetPayLoad(accessToken);

            var exitUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if(exitUser != null)
                return false;

            var user = _mapper.Map<User>(payload);
            user.AuthIssuer = AuthIssuer.GOOGLE;
            user.PasswordHash = "Google".PasswordHash();
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GoogleAuthorization(string accessToken)
        {
            var payload = await GetPayLoad(accessToken);

            if (payload == null)
                throw new Exception("incorrect Token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
                return null;

            var roles = new List<string> { user.Role.RoleName };

            string token = _tokenRepository.GenerateToken(user, roles);

            return token;
        }

        public async Task<User> GetGraphData(string accessToken)
        {
            WebRequest webRequest = WebRequest.Create("" + accessToken);
            FacebookGraphData facebookGraphData;

            using (WebResponse webResponse = webRequest.GetResponse())
            {
                using Stream stream = webResponse.GetResponseStream();
                facebookGraphData = await JsonSerializer.DeserializeAsync<FacebookGraphData>(stream);
            }

            if (facebookGraphData == null)
                throw new Exception("Incorrect Token");

            var user = new User() { Email = facebookGraphData.Email };
            return user;
        }

        public async Task<bool> FaceBookRegister(string accesToken)
        {
            var user = await GetGraphData(accesToken);
            if (user == null)
                return false;

            user.AuthIssuer = AuthIssuer.FACEBOOK;
            user.PasswordHash = "Facebook".PasswordHash();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> FaceBookAuthorization(string accessToken)
        {
            var userFromFacebook = await GetGraphData(accessToken);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userFromFacebook.Email);


            if (user == null)
                throw new Exception("Not Registered This Email");

            var roles = new List<string> { user.Role.RoleName };

            string token = _tokenRepository.GenerateToken(user, roles);


            return token;

        }
    }
}
