using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using BasicWebNovelAPI.Extensions;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class RatingRepository : IRatingRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public RatingRepository(
            BasicWebNovelContext context, 
            IMapper mapper,
            IDistributedCache cache) 
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
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
            
            // Clear cache for rating data
            var cacheKey = $"novel_rating_{novelId}";
            await _cache.SafeRemoveAsync(cacheKey);
            
            // Also clear cache for novels by rating
            await _cache.SafeRemoveAsync("novels_by_rating");

            return true;
        }

        public async Task<double> GetNovelRatingAsync(int novelId)
        {
            var cacheKey = $"novel_rating_{novelId}";
            var cachedRating = await _cache.GetValue<double?>(cacheKey);
            if (cachedRating.HasValue)
            {
                return cachedRating.Value;
            }
            
            var ratings = await _context.Ratings
                .Where(r => r.NovelId == novelId)
                .Select(r => r.Value)
                .ToListAsync();

            if (!ratings.Any())
                return 0;

            var averageRating = Math.Round(ratings.Average(), 2);
            
            // Cache the result
            await _cache.SetValue(cacheKey, averageRating);

            return averageRating;
        }
        
        public async Task<List<GetNovelDto>> GetMostPopularNovelsLastWeekAsync(int limit = 10)
        {
            var cacheKey = $"popular_novels_last_week_{limit}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);
            
            if (cachedNovels != null)
            {
                return cachedNovels;
            }
            
            // Get date one week ago
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            
            // Get novels with views from the last week, ordered by view count
            var novels = await _context.NovelViews
                .Where(v => v.ViewedAt >= oneWeekAgo)
                .GroupBy(v => v.NovelId)
                .Select(g => new 
                { 
                    NovelId = g.Key, 
                    ViewCount = g.Count() 
                })
                .OrderByDescending(x => x.ViewCount)
                .Take(limit)
                .Join(
                    _context.Novels.Include(n => n.Chapters).Include(n => n.NovelGenres).ThenInclude(ng => ng.Genre),
                    v => v.NovelId,
                    n => n.Id,
                    (v, n) => n
                )
                .ToListAsync();
                
            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);
            
            // Populate total chapters
            foreach (var novelDto in novelDtos)
            {
                var novel = novels.FirstOrDefault(n => n.Id == novelDto.Id);
                novelDto.TotalChapters = novel?.Chapters?.Count ?? 0;
            }
            
            // Cache the result
            await _cache.SetValue(cacheKey, novelDtos, TimeSpan.FromHours(1)); // Short cache time for popular items
            
            return novelDtos;
        }
        
        public async Task<List<GetNovelDto>> GetNovelsByRatingAsync(int limit = 10)
        {
            var cacheKey = $"novels_by_rating_{limit}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);
            
            if (cachedNovels != null)
            {
                return cachedNovels;
            }
            
            // Get novels with ratings, ordered by average rating
            var novelsWithRatings = await _context.Ratings
                .GroupBy(r => r.NovelId)
                .Select(g => new 
                { 
                    NovelId = g.Key, 
                    AverageRating = g.Average(r => r.Value) 
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(limit)
                .Join(
                    _context.Novels.Include(n => n.Chapters).Include(n => n.NovelGenres).ThenInclude(ng => ng.Genre),
                    r => r.NovelId,
                    n => n.Id,
                    (r, n) => new { Novel = n, AverageRating = r.AverageRating }
                )
                .ToListAsync();
                
            var novels = novelsWithRatings.Select(n => n.Novel).ToList();
            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);
            
            // Populate total chapters
            foreach (var novelDto in novelDtos)
            {
                var novel = novels.FirstOrDefault(n => n.Id == novelDto.Id);
                novelDto.TotalChapters = novel?.Chapters?.Count ?? 0;
            }
            
            // Cache the result
            await _cache.SetValue(cacheKey, novelDtos);
            
            return novelDtos;
        }
    }
}
