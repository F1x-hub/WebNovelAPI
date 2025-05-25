using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;
using Microsoft.AspNetCore.Http;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IChapterRepository
    {
        Task<GetChapterDto> AddChapterToNovelAsync(int novelId, int userId, CreateChapterDto chapterDto);
        Task<bool> UpdateChapterAsync(int novelId, int userId, int chapterId, UpdateChapterDto updateChapterDto);
        Task<bool> DeleteChapterAsync(int novelId, int userId, int chapterId);
        Task<List<GetChapterDto>> GetAllChaptersAsync(int novelId);
        Task<GetChapterDto?> GetChapterAsync(int novelId, int chapterNumber, int userId);
        Task<bool> UpdateLastReadChapterAsync(int userId, int novelId, int chapterNumber);
        Task<int> GetLastReadChapterAsync(int userId, int novelId);
        Task<string> UploadPdfToS3Async(IFormFile pdfFile, int userId, int novelId);
        Task<Stream> GetPdfFromS3Async(string pdfKey);
        Task DeletePdfFromS3Async(string pdfKey);
    }
}
