using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class RatingRepository : IRatingRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public RatingRepository(BasicWebNovelContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> RateNovelAsync(int novelId, int userId, double ratingValue)
        {
            
            if (ratingValue < 1 || ratingValue > 5)
                return false;

            
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.NovelId == novelId && r.UserId == userId);

            if (existingRating != null)
            {
                
                existingRating.Value = ratingValue;
                _context.Ratings.Update(existingRating);
            }
            else
            {
                
                var newRating = new Rating
                {
                    Value = ratingValue,
                    UserId = userId,
                    NovelId = novelId
                };

                await _context.Ratings.AddAsync(newRating);
            }

            
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<double> GetNovelRatingAsync(int novelId)
        {
            
            var ratings = await _context.Ratings
                .Where(r => r.NovelId == novelId)
                .Select(r => r.Value)
                .ToListAsync();

            if (!ratings.Any())
                return 0;

            var averageRating = ratings.Average();

            return Math.Round(averageRating, 2);
        }

    }
}
