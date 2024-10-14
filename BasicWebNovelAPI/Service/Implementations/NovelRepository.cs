using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel;
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

        
        public async Task<List<GetNovelDto>> GetNovels()
        {
            var CacheKey = "novels";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(CacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(CacheKey)))
            {
                return cachedNovels;
            }

            var novels = await _context.Novels
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            await _cache.SetValue(CacheKey, novelDtos, TimeSpan.FromMinutes(10));

            return novelDtos;
        }

        
        public async Task<GetNovelDto> GetNovelById(int novelId)
        {
            var cacheKey = $"novel_{novelId}";
            var cachedNovels = await _cache.GetValue<GetNovelDto>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                
                return cachedNovels;
            }

            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                return null;

            var novelDtos = _mapper.Map<GetNovelDto>(novel);

            await _cache.SetValue(cacheKey, novelDtos, TimeSpan.FromMinutes(10));

            return novelDtos;
        }

        public async Task<List<GetNovelDto>> GetNovelByName(string title)
        {
            var cacheKey = $"novel_{title}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedNovels;
            }

            var novels = await _context.Novels
                .Where(n => n.Title.ToLower().Contains(title.ToLower()))
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            await _cache.SetValue(cacheKey,novelDtos, TimeSpan.FromMinutes(10));

            return novelDtos;
        }


        public async Task<List<GetNovelDto>> GetUserAllNovel(int userId)
        {
            var cacheKey = $"user_novels_{userId}";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedNovels;
            }
            var novels = await _context.Novels
                .Where(n => n.UserId == userId)
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            await _cache.SetValue(cacheKey, novelDtos, TimeSpan.FromMinutes(10));

            return novelDtos;

        }

        
        public async Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == createNovelDto.UserId);

            if (user == null)
                throw new Exception("User Not Found");

            var novel = _mapper.Map<Novel>(createNovelDto);
            novel.UserId = createNovelDto.UserId; 
            novel.PublishedDate = DateTime.Now;

            if (createNovelDto.GenreIds != null && createNovelDto.GenreIds.Count > 0)
            {
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
                }
            }


            await _context.Novels.AddAsync(novel);
            await _context.SaveChangesAsync();

            var novelDtos = _mapper.Map<GetNovelDto>(novel);

            return novelDtos;

        }

        

        public async Task<bool> UpdateNovel(int id, UpdateNovelDto updateNovelDto)
        {
            var existingNovel = await _context.Novels
                .Include (n => n.Chapters)
                .FirstOrDefaultAsync (n => n.Id == id);

            if(existingNovel == null)
                return false;

            _mapper.Map(updateNovelDto, existingNovel);

            _context.Novels.Update(existingNovel);
            await _context.SaveChangesAsync();

            return true;

        }

        
        public async Task<bool> DeleteNovel(int novelId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if(novel == null) 
                return false;

            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();

            return true;

        }

        
        
    }
}
