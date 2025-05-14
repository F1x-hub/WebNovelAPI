using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface INovelRepository
    {
        Task<List<GetNovelDto>> GetNovels(int pageNumber = 1, int pageSize = 10, int? genreId = null, NovelStatus? status = null, string sortBy = null);
        Task<GetNovelDto> GetNovelById(int novelId);
        Task<List<GetNovelDto>> GetNovelByName(string title, int? genreId = null, NovelStatus? status = null, string sortBy = null);
        Task<List<GetNovelDto>> GetUserAllNovel(int userId, int? genreId = null, NovelStatus? status = null, string sortBy = null);
        Task<bool> UpdateNovel(int novelId, int userId, UpdateNovelDto updateNovelDto);
        Task<bool> DeleteNovel(int novelId, int userId);
        Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto, int userId);
        Task<bool> IncrementNovelViews(int novelId, int userId, string ipAddress = null);
    }
}
