using BasicWebNovelAPI.Model.Dto.Novel.Novel;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IRatingRepository
    {
        Task<bool> RateNovelAsync(int novelId, int userId, double ratingValue);
        Task<double> GetNovelRatingAsync(int novelId);
        Task<List<GetNovelDto>> GetMostPopularNovelsLastWeekAsync(int limit = 10);
        Task<List<GetNovelDto>> GetNovelsByRatingAsync(int limit = 10);
    }
}
