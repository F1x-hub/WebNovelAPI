using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly BasicWebNovelContext _context;

        public UserRepository(BasicWebNovelContext context)
        {
            _context = context;
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


        public async Task<bool> UpdateUserAsync(User updatedUser)
        {
            _context.Users.Update(updatedUser);
            await _context.SaveChangesAsync();


            return true;
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
