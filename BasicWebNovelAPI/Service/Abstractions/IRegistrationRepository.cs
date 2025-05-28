using BasicWebNovelAPI.Model.Dto.User;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IRegistrationRepository
    {
        Task<GetUserDto> Registration(RegisterUserDto registerUserDto);
        
        Task<bool> FaceBookRegister(string accesToken);

        Task<bool> VerifyEmail(VerifyCodeDto verifyCodeDto);
    }
}
