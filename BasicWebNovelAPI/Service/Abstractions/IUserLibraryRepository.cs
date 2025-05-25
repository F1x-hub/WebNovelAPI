using BasicWebNovelAPI.Model.Dto.Novel.Library;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IUserLibraryRepository
    {
        Task<bool> AddNovelToUserLibraryAsync(int userId, int novelId, int lastReadChapter);
        Task<List<GetUserLibraryDto>> GetUserLibraryAsync(int userId);
        Task<bool> IsNovelInUserLibraryAsync(int userId, int novelId);
        Task<bool> UpdateLastReadChapterAsync(int userId, int novelId, int lastReadChapter);
        Task<bool> ResetAddedChapterAsync(int userId, int novelId);
    }
}
