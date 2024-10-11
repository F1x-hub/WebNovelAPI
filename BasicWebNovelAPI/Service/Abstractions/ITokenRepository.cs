using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface ITokenRepository
    {
        string GenerateToken(User user, List<string> roles);
    }
}
