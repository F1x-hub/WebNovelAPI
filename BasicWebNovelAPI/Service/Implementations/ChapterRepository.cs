using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public ChapterRepository(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if (novel == null)
                throw new Exception("Novel not found or access denied.");

            var chapterDtos = _mapper.Map<List<GetChapterDto>>(novel.Chapters.OrderBy(c => c.ChapterNumber));

            return chapterDtos;
        }

        public async Task<GetChapterDto?> GetChapterAsync(int novelId, int chapterNumber, int userId)
        {
            
            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);

            if (chapter == null)
            {
                return null; 
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

            return chapterDto;
        }
    }
}
