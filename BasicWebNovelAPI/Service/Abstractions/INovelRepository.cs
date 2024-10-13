using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface INovelRepository
    {
        Task<List<GetNovelDto>> GetNovels();
        Task<GetNovelDto> GetNovelById(int novelId);
        Task<List<GetNovelDto>> GetNovelByName(string title);
        Task<List<GetNovelDto>> GetUserAllNovel(int userId);
        Task<bool> UpdateNovel(int id, UpdateNovelDto updateNovelDto);
        Task<bool> DeleteNovel(int novelId);
        Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto);
        Task<GetGenreDto> CreateGenre(CreateGenreDto createGenreDto);
        Task<Chapter> AddChapterToNovelAsync(int novelId, CreateChapterDto createChapterDto);
        Task<bool> UpdateChapter(int novelId, int chapterId, Chapter updatedChapter);
        Task<bool> DeleteChapter(int novelId, int chapterId);
    }
}
