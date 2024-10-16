using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public UserRepository(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


        public async Task<User> GetUserIdAsync(int userId)
        {
            return await _context.Users
                         .FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto userDto)
        {
            var user = await _context.Users
                         .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false; 
            }

            
            _mapper.Map(userDto, user);

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false; 
            }
        }


        public async Task<User> DeleteUserIdAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }
}
