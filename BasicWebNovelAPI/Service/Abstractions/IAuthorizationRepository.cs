using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IAuthorizationRepository
    {
        Task<List<User>> GetUsers();
        Task<User> GetUserId(int userId);
        Task<bool> UpdateUser(User updatedUser);
        Task<User> DeleteUserId(int userId);
        Task<GetUserDto> Registration(RegisterUserDto registerUserDto);
        Task AddUserImagesAsync(int userId, IFormFile? imageFiles);
        Task<string> LogIn(GetLoginDto getLoginDto);
        Task<string> VerifyCode(VerifyCodeDto verifyCodeDto);
        Task<bool> GoogleRegister(string accessToken);
        Task<string> GoogleAuthorization(string accessToken);
        Task<bool> FaceBookRegister(string accesToken);
        Task<string> FaceBookAuthorization(string accessToken);

    }
}
