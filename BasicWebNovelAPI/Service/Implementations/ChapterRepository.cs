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
        public async Task<Chapter> AddChapterToNovelAsync(int novelId, CreateChapterDto chapterDto)
        {
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
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

            return chapter;
        }

        // Update an existing chapter in a novel
        public async Task<bool> UpdateChapterAsync(int novelId, int chapterId, Chapter updatedChapter)
        {
            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                throw new Exception("Novel not found");

            var chapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                throw new Exception("Chapter not found");

            chapter.Title = updatedChapter.Title;
            chapter.Content = updatedChapter.Content;
            chapter.ChapterNumber = updatedChapter.ChapterNumber;

            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete a chapter from a novel
        public async Task<bool> DeleteChapterAsync(int novelId, int chapterId)
        {
            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                throw new Exception("Novel not found");

            var chapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                return false;

            novel.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
