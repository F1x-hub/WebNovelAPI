using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthorizationRepository _authorizationRepository;

        public RegistrationRepository(BasicWebNovelContext context, IMapper mapper, IAuthorizationRepository authorizationRepository)
        {
            _context = context;
            _mapper = mapper;
            _authorizationRepository = authorizationRepository;
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

        public async Task<bool> GoogleRegister(string accessToken)
        {
            var payload = await _authorizationRepository.GetPayLoad(accessToken);

            var exitUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (exitUser != null)
                return false;

            var user = _mapper.Map<User>(payload);
            user.AuthIssuer = AuthIssuer.GOOGLE;
            user.PasswordHash = "Google".PasswordHash();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FaceBookRegister(string accesToken)
        {
            var user = await _authorizationRepository.GetGraphData(accesToken);
            if (user == null)
                return false;

            user.AuthIssuer = AuthIssuer.FACEBOOK;
            user.PasswordHash = "Facebook".PasswordHash();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
