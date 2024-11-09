using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface INovelRepository
    {
        Task<List<GetNovelDto>> GetNovels();
        Task<GetNovelDto> GetNovelById(int novelId);
        Task<List<GetNovelDto>> GetNovelByName(string title);
        Task<List<GetNovelDto>> GetUserAllNovel(int userId);
        Task<bool> UpdateNovel(int id, int userId, UpdateNovelDto updateNovelDto);
        Task<bool> DeleteNovel(int novelId, int userId);
        Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto, int userId);
        
        
        
    }
}
