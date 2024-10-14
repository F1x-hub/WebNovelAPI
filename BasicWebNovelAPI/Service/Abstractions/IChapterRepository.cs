using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IChapterRepository
    {
        Task<Chapter> AddChapterToNovelAsync(int novelId, CreateChapterDto chapterDto);
        Task<bool> UpdateChapterAsync(int novelId, int chapterId, Chapter updatedChapter);
        Task<bool> DeleteChapterAsync(int novelId, int chapterId);
    }
}
