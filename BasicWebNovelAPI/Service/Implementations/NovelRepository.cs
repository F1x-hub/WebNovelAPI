using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class NovelRepository : INovelRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        

        public NovelRepository(BasicWebNovelContext context, IMapper mapper, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;   
        }

        
        public async Task<List<GetNovelDto>> GetNovels(int pageNumber = 1, int pageSize = 10, int? genreId = null, NovelStatus? status = null, string sortBy = null)
        {
            var cacheKey = $"novels_page{pageNumber}_size{pageSize}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedNovels;
            }

            var query = _context.Novels.AsQueryable();
            
            // Apply genre filter if provided
            if (genreId.HasValue)
            {
                query = query.Where(n => n.NovelGenres.Any(ng => ng.GenreId == genreId.Value));
            }
            
            // Apply status filter if provided
            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }
            
            // Load related data
            query = query.Include(n => n.Chapters)
                         .Include(n => n.NovelGenres)
                         .ThenInclude(ng => ng.Genre)
                         .Include(n => n.Ratings);

            // Special case for rating sorting
            bool needRatingSort = sortBy?.ToLower() == "rating";
            if (!needRatingSort)
            {
                // Apply sorting for all cases except rating
                ApplySorting(ref query, sortBy);
            }
            
            // Apply pagination
            var novels = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);
            
            // Populate TotalChapters property and average rating
            foreach (var novelDto in novelDtos)
            {
                var novel = novels.FirstOrDefault(n => n.Id == novelDto.Id);
                novelDto.TotalChapters = novel?.Chapters?.Count ?? 0;
                
                // Calculate average rating if ratings exist
                novelDto.AverageRating = novel?.Ratings != null && novel.Ratings.Any()
                    ? novel.Ratings.Average(r => r.Value)
                    : 0;
            }

            // Special handling for rating sort after materialization
            if (needRatingSort)
            {
                novelDtos = novelDtos.OrderByDescending(n => n.AverageRating).ToList();
            }

            await _cache.SetValue(cacheKey, novelDtos);

            return novelDtos;
        }

        
        public async Task<GetNovelDto> GetNovelById(int novelId)
        {
            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .Include(n => n.NovelGenres) 
                .ThenInclude(ng => ng.Genre) 
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                return null;

            var novelDto = _mapper.Map<GetNovelDto>(novel);
            
            // Populate TotalChapters property
            novelDto.TotalChapters = novel.Chapters?.Count ?? 0;

            // Store in cache
            var cacheKey = $"novel_{novelId}";
            await _cache.SetValue(cacheKey, novelDto);

            return novelDto;
        }

        public async Task<List<GetNovelDto>> GetNovelByName(string title, int? genreId = null, NovelStatus? status = null, string sortBy = null)
        {
            var cacheKey = $"novel_{title}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedNovels;
            }

            var query = _context.Novels.AsQueryable();
            
            // Apply name filter
            query = query.Where(n => n.Title.ToLower().Contains(title.ToLower()));
            
            // Apply genre filter if provided
            if (genreId.HasValue)
            {
                query = query.Where(n => n.NovelGenres.Any(ng => ng.GenreId == genreId.Value));
            }
            
            // Apply status filter if provided
            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }
            
            // Load related data
            query = query.Include(n => n.Chapters)
                         .Include(n => n.NovelGenres)
                         .ThenInclude(ng => ng.Genre)
                         .Include(n => n.Ratings);

            // Special case for rating sorting
            bool needRatingSort = sortBy?.ToLower() == "rating";
            if (!needRatingSort)
            {
                // Apply sorting for all cases except rating
                ApplySorting(ref query, sortBy);
            }

            var novels = await query.ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);
            
            // Populate TotalChapters property and average rating
            foreach (var novelDto in novelDtos)
            {
                var novel = novels.FirstOrDefault(n => n.Id == novelDto.Id);
                novelDto.TotalChapters = novel?.Chapters?.Count ?? 0;
                
                // Calculate average rating if ratings exist
                novelDto.AverageRating = novel?.Ratings != null && novel.Ratings.Any()
                    ? novel.Ratings.Average(r => r.Value)
                    : 0;
            }

            // Special handling for rating sort after materialization
            if (needRatingSort)
            {
                novelDtos = novelDtos.OrderByDescending(n => n.AverageRating).ToList();
            }

            await _cache.SetValue(cacheKey, novelDtos);

            return novelDtos;
        }


        public async Task<List<GetNovelDto>> GetUserAllNovel(int userId, int? genreId = null, NovelStatus? status = null, string sortBy = null)
        {
            var cacheKey = $"user_novels_{userId}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedNovels;
            }
            
            var query = _context.Novels.AsQueryable();
            
            // Filter by user
            query = query.Where(n => n.UserId == userId);
            
            // Apply genre filter if provided
            if (genreId.HasValue)
            {
                query = query.Where(n => n.NovelGenres.Any(ng => ng.GenreId == genreId.Value));
            }
            
            // Apply status filter if provided
            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }
            
            // Load related data
            query = query.Include(n => n.Chapters)
                         .Include(n => n.NovelGenres)
                         .ThenInclude(ng => ng.Genre)
                         .Include(n => n.Ratings);

            // Special case for rating sorting
            bool needRatingSort = sortBy?.ToLower() == "rating";
            if (!needRatingSort)
            {
                // Apply sorting for all cases except rating
                ApplySorting(ref query, sortBy);
            }

            var novels = await query.ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);
            
            // Populate TotalChapters property and average rating
            foreach (var novelDto in novelDtos)
            {
                var novel = novels.FirstOrDefault(n => n.Id == novelDto.Id);
                novelDto.TotalChapters = novel?.Chapters?.Count ?? 0;
                
                // Calculate average rating if ratings exist
                novelDto.AverageRating = novel?.Ratings != null && novel.Ratings.Any()
                    ? novel.Ratings.Average(r => r.Value)
                    : 0;
            }

            // Special handling for rating sort after materialization
            if (needRatingSort)
            {
                novelDtos = novelDtos.OrderByDescending(n => n.AverageRating).ToList();
            }

            await _cache.SetValue(cacheKey, novelDtos);

            return novelDtos;
        }

        
        public async Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User Not Found");

            if (createNovelDto.GenreIds == null || createNovelDto.GenreIds.Count == 0)
            {
                throw new Exception("At least one genre must be selected.");
            }


            var novel = _mapper.Map<Novel>(createNovelDto);
            novel.UserId = user.Id;
            novel.PublishedDate = DateTime.Now;
            novel.Views = 0;
            novel.Status = createNovelDto.Status;
            novel.IsAdultContent = createNovelDto.IsAdultContent;

            novel.NovelGenres = new List<NovelGenre>();

            
            foreach (var genreId in createNovelDto.GenreIds)
            {
                var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == genreId);
                if (genre != null)
                {
                    novel.NovelGenres.Add(new NovelGenre
                    {
                        GenreId = genre.Id,
                        Novel = novel
                    });
                }
                else
                {
                    throw new Exception($"Genre with ID {genreId} does not exist.");
                }
            }


            await _context.Novels.AddAsync(novel);
            await _context.SaveChangesAsync();

            var novelDto = _mapper.Map<GetNovelDto>(novel);
            // Since this is a new novel, it has 0 chapters
            novelDto.TotalChapters = 0;

            return novelDto;
        }

        public async Task<bool> UpdateNovel(int novelId, int userId, UpdateNovelDto updateNovelDto)
        {
            var existingNovel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if(existingNovel == null)
                return false;

            _mapper.Map(updateNovelDto, existingNovel);
            
            // Handle optional properties
            if (updateNovelDto.Views.HasValue)
            {
                existingNovel.Views = updateNovelDto.Views.Value;
            }
            
            if (updateNovelDto.Status.HasValue)
            {
                existingNovel.Status = updateNovelDto.Status.Value;
            }
            
            if (updateNovelDto.IsAdultContent.HasValue)
            {
                existingNovel.IsAdultContent = updateNovelDto.IsAdultContent.Value;
            }

            _context.Novels.Update(existingNovel);
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this novel
            await _cache.RemoveAsync($"novel_{novelId}");

            return true;
        }

        
        public async Task<bool> DeleteNovel(int novelId, int userId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if(novel == null) 
                return false;

            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();
            
            // Clear cache for this novel and related queries
            await _cache.RemoveAsync($"novel_{novelId}");
            // Also consider clearing other caches that might contain this novel

            return true;
        }
        
        public async Task<bool> IncrementNovelViews(int novelId, int userId, string ipAddress = null)
        {
            // Check if the novel exists
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
                return false;
            
            // Start a transaction to ensure operations are atomic
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    bool shouldIncrement = false;
                    
                    // Handle anonymous users (identified by IP)
                    if (userId <= 0 && !string.IsNullOrEmpty(ipAddress))
                    {
                        // Check if this IP has viewed the novel recently (last 24 hours)
                        var recentIpView = await _context.NovelViews
                            .Where(v => v.NovelId == novelId && 
                                   v.IpAddress == ipAddress && 
                                   v.ViewedAt > DateTime.Now.AddHours(-24))
                            .FirstOrDefaultAsync();
                            
                        if (recentIpView == null)
                        {
                            // IP hasn't viewed this novel recently, record view and increment
                            _context.NovelViews.Add(new NovelView
                            {
                                NovelId = novelId,
                                IpAddress = ipAddress,
                                ViewedAt = DateTime.Now
                            });
                            
                            shouldIncrement = true;
                        }
                    }
                    // Handle logged-in users
                    else if (userId > 0)
                    {
                        // Check if user has viewed this novel recently (last 24 hours)
                        var recentUserView = await _context.NovelViews
                            .Where(v => v.NovelId == novelId && 
                                   v.UserId == userId && 
                                   v.ViewedAt > DateTime.Now.AddHours(-24))
                            .FirstOrDefaultAsync();
                            
                        if (recentUserView == null)
                        {
                            // User hasn't viewed this novel recently, record view and increment
                            _context.NovelViews.Add(new NovelView
                            {
                                NovelId = novelId,
                                UserId = userId,
                                ViewedAt = DateTime.Now
                            });
                            
                            shouldIncrement = true;
                        }
                    }
                    
                    // Increment the view count if needed
                    if (shouldIncrement)
                    {
                        novel.Views++;
                        _context.Novels.Update(novel);
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    // Clear cache for this novel
                    await _cache.RemoveAsync($"novel_{novelId}");
                    
                    return true;
                }
                catch (Exception ex)
                {
                    // Roll back if anything fails
                    await transaction.RollbackAsync();
                    // Log the exception
                    Console.WriteLine($"Error incrementing views: {ex.Message}");
                    return false;
                }
            }
        }

        // Helper method to apply sorting
        private void ApplySorting(ref IQueryable<Novel> query, string sortBy)
        {
            switch (sortBy?.ToLower())
            {
                case "popular":
                    query = query.OrderByDescending(n => n.Views);
                    break;
                case "rating":
                    // This approach can be translated to SQL - first get all novels
                    // We'll apply rating sorting after materialization
                    break;
                case "newest":
                    query = query.OrderByDescending(n => n.PublishedDate);
                    break;
                case "a-z":
                    query = query.OrderBy(n => n.Title);
                    break;
                // Default sorting - newest first
                default:
                    query = query.OrderByDescending(n => n.PublishedDate);
                    break;
            }
        }
    }
}
