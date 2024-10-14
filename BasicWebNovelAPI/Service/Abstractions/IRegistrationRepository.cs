using BasicWebNovelAPI.Model.Dto.User;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IRegistrationRepository
    {
        Task<GetUserDto> Registration(RegisterUserDto registerUserDto);
        Task<bool> GoogleRegister(string accessToken);
        Task<bool> FaceBookRegister(string accesToken);
    }
}
