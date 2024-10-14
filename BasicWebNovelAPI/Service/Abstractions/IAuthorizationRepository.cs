using BasicWebNovelAPI.Model.Dto.User;
using BasicWebNovelAPI.Model.UserManagement;
using Google.Apis.Auth;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IAuthorizationRepository
    {
        
        
        Task<string> LogIn(GetLoginDto getLoginDto);
        Task<string> VerifyCode(VerifyCodeDto verifyCodeDto);
        
        Task<string> GoogleAuthorization(string accessToken);
        
        Task<string> FaceBookAuthorization(string accessToken);

        Task<GoogleJsonWebSignature.Payload> GetPayLoad(string token);
        Task<User> GetGraphData(string accessToken);

    }
}
