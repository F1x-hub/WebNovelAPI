using BasicWebNovelAPI.Model.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserIdAsync(int userId);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<User> DeleteUserIdAsync(int userId);
    }
}
