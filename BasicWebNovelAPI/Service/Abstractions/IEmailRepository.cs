namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IEmailRepository
    {
        Task<string> SendToEmail(string email, string text);
    }
}
