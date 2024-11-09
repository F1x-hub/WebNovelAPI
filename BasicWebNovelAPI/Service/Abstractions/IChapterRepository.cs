using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IChapterRepository
    {
        Task<GetChapterDto> AddChapterToNovelAsync(int novelId, int userId, CreateChapterDto chapterDto);
        Task<bool> UpdateChapterAsync(int novelId, int userId, int chapterId, UpdateChapterDto updateChapterDto);
        Task<bool> DeleteChapterAsync(int novelId, int userId, int chapterId);
        Task<List<GetChapterDto>> GetAllChaptersAsync(int novelId, int userId);
        Task<GetChapterDto?> GetChapterAsync(int novelId, int chapterNumber, int userId);
    }
}
