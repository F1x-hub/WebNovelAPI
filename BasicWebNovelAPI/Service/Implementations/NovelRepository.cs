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

        
        public async Task<NovelPagedResult> GetNovels(int pageNumber = 1, int pageSize = 10, int? genreId = null, NovelStatus? status = null, string? sortBy = null)
        {
            var cacheKey = $"novels_page{pageNumber}_size{pageSize}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedResult = await _cache.GetValue<NovelPagedResult>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
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
            
            // Calculate total count before applying pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
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

            var result = new NovelPagedResult
            {
                Novels = novelDtos,
                TotalPages = totalPages,
                TotalItems = totalItems,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };

            await _cache.SetValue(cacheKey, result);

            return result;
        }

        
        public async Task<GetNovelDto?> GetNovelById(int novelId)
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

        public async Task<List<GetNovelDto>> GetNovelByName(string title, int? genreId = null, NovelStatus? status = null, string? sortBy = null)
        {
            var cacheKey = $"novel_{title}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (cachedNovels != null)
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


        public async Task<List<GetNovelDto>> GetUserAllNovel(int userId, int? genreId = null, NovelStatus? status = null, string? sortBy = null)
        {
            var cacheKey = $"user_novels_{userId}_genre{genreId}_status{status}_sort{sortBy}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (cachedNovels != null)
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

            // Set a shorter cache duration for faster updates
            await _cache.SetValue(cacheKey, novelDtos, TimeSpan.FromSeconds(30));

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

            // Check if a novel with the same title already exists
            bool titleExists = await _context.Novels.AnyAsync(n => n.Title.ToLower() == createNovelDto.Title.ToLower());
            if (titleExists)
            {
                throw new Exception("A novel with this title already exists. Please choose a different title.");
            }

            var novel = _mapper.Map<Novel>(createNovelDto);
            novel.UserId = user.Id;
            
            // Set publication date to UTC+4
            TimeZoneInfo utcPlus4 = TimeZoneInfo.CreateCustomTimeZone("UTC+4", TimeSpan.FromHours(4), "UTC+4", "UTC+4");
            novel.PublishedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcPlus4);
            
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

            // Invalidate relevant cache entries
            try
            {
                // Invalidate paginated novels cache - using a pattern to clear all pages
                await InvalidateNovelListCaches();
                
                // Invalidate user's novels cache
                await _cache.SafeRemoveAsync($"user_novels_{userId}");
            }
            catch
            {
                // Continue if cache operations fail
            }

            return novelDto;
        }

        public async Task<bool> UpdateNovel(int novelId, int userId, UpdateNovelDto updateNovelDto)
        {
            var existingNovel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if(existingNovel == null)
                return false;

            // Check if title is being updated and if the new title already exists
            if (!string.IsNullOrEmpty(updateNovelDto.Title) && 
                existingNovel.Title.ToLower() != updateNovelDto.Title.ToLower())
            {
                bool titleExists = await _context.Novels
                    .Where(n => n.Id != novelId)
                    .AnyAsync(n => n.Title.ToLower() == updateNovelDto.Title.ToLower());
                    
                if (titleExists)
                {
                    throw new Exception("A novel with this title already exists. Please choose a different title.");
                    
                }
            }

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
            
            // Invalidate cache for this novel and related caches
            try
            {
                // Invalidate specific novel cache
                await _cache.SafeRemoveAsync($"novel_{novelId}");
                
                // Invalidate paginated novels cache
                await InvalidateNovelListCaches();
                
                // Invalidate user's novels cache
                await _cache.SafeRemoveAsync($"user_novels_{userId}");
                
                // If title was updated, invalidate search caches
                if (!string.IsNullOrEmpty(updateNovelDto.Title))
                {
                    // We can't target specific search cache keys, so use pattern matching if available
                    // For now, we'll just invalidate all novel search caches
                    await InvalidateNovelSearchCaches();
                }
            }
            catch
            {
                // Continue if cache operations fail
            }

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
            try
            {
                // Invalidate specific novel cache
                await _cache.SafeRemoveAsync($"novel_{novelId}");
                
                // Invalidate paginated novels cache
                await InvalidateNovelListCaches();
                
                // Invalidate user's novels cache
                await _cache.SafeRemoveAsync($"user_novels_{userId}");
                
                // Invalidate search caches
                await InvalidateNovelSearchCaches();
            }
            catch
            {
                // Continue if cache operations fail
            }

            return true;
        }
        
        public async Task<bool> IncrementNovelViews(int novelId, int userId, string? ipAddress = null)
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
                            await _context.NovelViews.AddAsync(new NovelView
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
                            await _context.NovelViews.AddAsync(new NovelView
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
                    
                    // Clear cache for this novel - moved after transaction is committed
                    try
                    {
                        await _cache.SafeRemoveAsync($"novel_{novelId}");
                    }
                    catch
                    {
                        // Silently continue if cache operation fails
                    }
                    
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
        private void ApplySorting(ref IQueryable<Novel> query, string? sortBy)
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

        // Update the helper method to invalidate novel list caches
        private async Task InvalidateNovelListCaches()
        {
            // Clear all paginated novel caches
            for (int page = 1; page <= 5; page++)
            {
                await _cache.SafeRemoveAsync($"novels_page{page}_size10_genrenull_statusnull_sortnull");
                await _cache.SafeRemoveAsync($"novels_page{page}_size20_genrenull_statusnull_sortnull");
            }
            
            // Clear user novel caches with different parameter combinations
            // First get all users who have novels
            var userIds = await _context.Novels
                .Select(n => n.UserId)
                .Distinct()
                .Take(100) // Limit to avoid too many operations
                .ToListAsync();
            
            foreach (var userId in userIds)
            {
                // Clear base user novels cache
                await _cache.SafeRemoveAsync($"user_novels_{userId}");
                
                // Clear with common sortBy parameters
                await _cache.SafeRemoveAsync($"user_novels_{userId}_genrenull_statusnull_sortpopular");
                await _cache.SafeRemoveAsync($"user_novels_{userId}_genrenull_statusnull_sortrating");
                await _cache.SafeRemoveAsync($"user_novels_{userId}_genrenull_statusnull_sortnewest");
                await _cache.SafeRemoveAsync($"user_novels_{userId}_genrenull_statusnull_sorta-z");
                
                // Clear with status combinations
                foreach (var status in System.Enum.GetValues(typeof(NovelStatus)))
                {
                    await _cache.SafeRemoveAsync($"user_novels_{userId}_genrenull_status{status}_sortnull");
                }
            }
        }

        // New helper method to invalidate novel search caches
        private Task InvalidateNovelSearchCaches()
        {
            // Since we don't know what search terms have been used,
            // we would need pattern matching support from the cache provider
            // For now, we'll just rely on cache expiration for search results
            return Task.CompletedTask;
        }
    }
}
