using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public ChapterRepository(BasicWebNovelContext context, IMapper mapper, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<GetChapterDto> AddChapterToNovelAsync(int novelId, int userId, CreateChapterDto chapterDto)
        {
            var novel = await _context.Novels.FirstOrDefaultAsync(u => u.Id == novelId && u.UserId == userId);
            if (novel == null || novel.UserId != userId)
            {
                throw new KeyNotFoundException($"Novel with ID {novelId} not found.");
            }


            var lastChapter = await _context.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderByDescending(c => c.ChapterNumber)
                .FirstOrDefaultAsync();


            int nextChapterNumber = lastChapter?.ChapterNumber + 1 ?? 1;


            var chapter = _mapper.Map<Chapter>(chapterDto);
            chapter.NovelId = novelId;
            
            chapter.ChapterNumber = nextChapterNumber;

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            var newChapterDto = _mapper.Map<GetChapterDto>(chapter);

            return newChapterDto;
        }

        
        public async Task<bool> UpdateChapterAsync(int novelId,int userId, int chapterId, UpdateChapterDto updateChapterDto)
        {
            var novel = await _context.Novels
        .Include(n => n.Chapters)
        .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            
            if (novel == null)
                return false;

            
            var existingChapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);

            
            if (existingChapter == null)
                return false;

            
            _mapper.Map(updateChapterDto, existingChapter);

            
            _context.Chapters.Update(existingChapter);
            await _context.SaveChangesAsync();

            return true;
        }

        
        public async Task<bool> DeleteChapterAsync(int novelId, int userId, int chapterId)
        {
            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            
            if (novel == null)
                return false;

            
            var chapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);

            
            if (chapter == null)
                return false;

            
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<GetChapterDto>> GetAllChaptersAsync(int novelId, int userId)
        {
            var cacheKey = $"chapters_{novelId}_{userId}";
            var cachedChapters = await _cache.GetValue<List<GetChapterDto>>(cacheKey);
            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedChapters;
            }

            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if (novel == null)
                throw new Exception("Novel not found or access denied.");

            var readChapters = await _context.UserChapterReads
                                      .Where(ur => ur.UserId == userId && ur.Chapter.NovelId == novelId)
                                      .ToDictionaryAsync(ur => ur.ChapterId, ur => ur.IsRead);

            
            var chapterDtos = novel.Chapters
                                   .OrderBy(c => c.ChapterNumber)
                                   .Select(chapter =>
                                   {
                                       var dto = _mapper.Map<GetChapterDto>(chapter);
                                       dto.IsRead = readChapters.ContainsKey(chapter.Id) && readChapters[chapter.Id];
                                       return dto;
                                   })
                                   .ToList();

            await _cache.SetValue(cacheKey, chapterDtos);

            return chapterDtos;
        }

        public async Task<GetChapterDto?> GetChapterAsync(int novelId, int chapterNumber, int userId)
        {
            var cacheKey = $"chapter_{novelId}_{chapterNumber}_{userId}";
            var cachedChapters = await _cache.GetValue<GetChapterDto>(cacheKey);
            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedChapters;
            }

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);

            if (chapter == null)
            {
                return null; 
            }

            var userChapterRead = await _context.UserChapterReads
                .FirstOrDefaultAsync(urc => urc.UserId == userId && urc.ChapterId == chapter.Id);

            if (userChapterRead == null)
            {
                userChapterRead = new UserChapterRead()
                {
                    UserId = userId,
                    ChapterId = chapter.Id,
                    IsRead = true
                };

                _context.UserChapterReads.Add(userChapterRead);
            }
            else if (!userChapterRead.IsRead)
            {
                userChapterRead.IsRead = true;
                _context.UserChapterReads.Add(userChapterRead);
            }

            
            var userLibraryEntry = await _context.UserLibraries
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.NovelId == novelId);

            if (userLibraryEntry != null)
            {
                
                userLibraryEntry.LastReadChapter = chapterNumber;
                _context.UserLibraries.Update(userLibraryEntry);
                await _context.SaveChangesAsync();
            }

            

            
            var chapterDto = _mapper.Map<GetChapterDto>(chapter);
            chapterDto.IsRead = userChapterRead?.IsRead ?? false;

            await _cache.SetValue(cacheKey, chapterDto);

            await _context.SaveChangesAsync();

            return chapterDto;
        }
    }
}
