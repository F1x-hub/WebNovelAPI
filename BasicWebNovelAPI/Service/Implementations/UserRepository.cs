using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
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
        private readonly IEmailRepository _emailRepository;

        public UserRepository(BasicWebNovelContext context, IMapper mapper, IEmailRepository emailRepository)
        {
            _context = context;
            _mapper = mapper;
            _emailRepository = emailRepository;
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
        
        // Password recovery - Step 1: Request reset
        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                throw new Exception("User with this email address not found");
            
            // Generate a random 6-digit code
            Random random = new Random();
            string code = random.Next(100000, 999999).ToString();
            
            // Store the code and set expiration (1 hour from now)
            user.TemporaryCode = code;
            user.CodeExpirationTime = DateTime.Now.AddHours(1);
            
            await _context.SaveChangesAsync();
            
            // Send email with the code
            string emailBody = $"Your password reset code is: {code}\n\nThis code will expire in 1 hour.";
            await _emailRepository.SendToEmail("iraklilagvilava975@gmail.com", emailBody);
            
            return "Password reset code has been sent to your email";
        }
        
        // Password recovery - Step 2: Reset with code
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            
            if (user == null)
                throw new Exception("User with this email address not found");
            
            // Verify code validity
            if (user.TemporaryCode != resetPasswordDto.Code)
                throw new Exception("Invalid verification code");
                
            // Check if code is expired
            if (!user.CodeExpirationTime.HasValue || DateTime.Now > user.CodeExpirationTime.Value)
                throw new Exception("Verification code has expired. Please request a new one");
            
            // Update password
            user.PasswordHash = resetPasswordDto.NewPassword.PasswordHash();
            
            // Clear the temporary code and expiration
            user.TemporaryCode = null;
            user.CodeExpirationTime = null;
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // Change password when logged in
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == changePasswordDto.UserId);
            
            if (user == null)
                throw new Exception("User not found");
            
            // Verify current password
            bool isCorrectPassword = changePasswordDto.CurrentPassword.PasswordVerify(user.PasswordHash);
            
            if (!isCorrectPassword)
                throw new Exception("Current password is incorrect");
            
            // Update password
            user.PasswordHash = changePasswordDto.NewPassword.PasswordHash();
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // Mark a user as an adult (18+)
        public async Task<bool> SetUserAsAdultAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
                throw new Exception("User not found");
            
            // Set IsAdult flag to true
            user.IsAdult = true;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
