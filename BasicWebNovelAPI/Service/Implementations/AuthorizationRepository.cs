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
        
        private readonly ITokenRepository _tokenRepository;
        private readonly IEmailRepository _emailRepository;
        

        public AuthorizationRepository(BasicWebNovelContext context, 
                                       ITokenRepository tokenRepository,
                                       IEmailRepository emailRepository)
        {
            _context = context;
            
            _tokenRepository = tokenRepository;
            
            _emailRepository = emailRepository;
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
