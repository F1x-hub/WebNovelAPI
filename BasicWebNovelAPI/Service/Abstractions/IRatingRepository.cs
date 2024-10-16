namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IRatingRepository
    {
        Task<bool> RateNovelAsync(int novelId, int userId, double ratingValue);
        Task<double> GetNovelRatingAsync(int novelId);
    }
}
