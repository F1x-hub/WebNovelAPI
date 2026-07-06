using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersAsync();
        Task<User?> GetUserIdAsync(int userId);
        Task<bool> UpdateUserAsync(int userId, UpdateUserDto userDto);
        Task<User?> DeleteUserIdAsync(int userId);
        
        // Password management methods
        Task<string> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        
        // Adult verification
        Task<bool> SetUserAsAdultAsync(int userId);
    }
}
