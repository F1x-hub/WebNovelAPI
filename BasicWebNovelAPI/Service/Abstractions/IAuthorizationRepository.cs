using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using Google.Apis.Auth;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IAuthorizationRepository
    {
        
        
        Task<string> LogIn(GetLoginDto getLoginDto);
        Task<string> VerifyCode(VerifyCodeDto verifyCodeDto);
        
        
        Task<string> FaceBookAuthorization(string accessToken);

        
        Task<User> GetGraphData(string accessToken);

        Task<string> GoogleAuthorization(string token);

        Task<User> GetGoogleUserData(string token);
    }
}
