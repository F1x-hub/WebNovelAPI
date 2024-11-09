using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;

        public CommentRepository(BasicWebNovelContext context, IDistributedCache cache, IMapper mapper)
        {
            _context = context;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<GetNovelCommentDto> SendNovelComment(CreateNovelCommentDto createNovelCommentDto, int userId, int novelId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User Not Found");


            var comment = _mapper.Map<NovelComments>(createNovelCommentDto);
            comment.UserId = userId;
            comment.NovelId = novelId;

            comment.PublishedDate = DateTime.Now;

            await _context.NovelComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var novelCommentDto = _mapper.Map<GetNovelCommentDto>(comment);

            return novelCommentDto;
        }

        public async Task<GetChapterCommentDto> SendChapterComment(CreateChapterCommentDto createChapterCommentDto, int userId, int chapterId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User Not Found");


            var comment = _mapper.Map<ChapterComments>(createChapterCommentDto);
            comment.UserId = userId;
            comment.ChapterId = chapterId;
            

            comment.PublishedDate = DateTime.Now;

            await _context.ChapterComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var chapterCommentDto = _mapper.Map<GetChapterCommentDto>(comment);

            return chapterCommentDto;
        }

        public async Task<List<GetNovelCommentDto>> GetAllCommentNovel(int novelId)
        {
            var cacheKey = $"comment_novel_{novelId}";
            var commentCached = await _cache.GetValue<List<GetNovelCommentDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return commentCached;
            }


            var novelComment = await _context.NovelComments
                .Where(n => n.NovelId == novelId)
                .ToListAsync();

            if (novelComment.Count <= 0)
            {
               
            }

            var novelCommentDto = _mapper.Map<List<GetNovelCommentDto>>(novelComment);

            await _cache.SetValue(cacheKey, novelCommentDto);

            return novelCommentDto;
        }

        public async Task<List<GetChapterCommentDto>> GetAllCommentChapter(int chapterId)
        {
            var cacheKey = $"comment_chapter_{chapterId}";
            var commentCached = await _cache.GetValue<List<GetChapterCommentDto>>(cacheKey);

            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return commentCached;
            }


            var chapterComment = await _context.ChapterComments
                .Where(n => n.ChapterId == chapterId)
                .ToListAsync();

            if (chapterComment.Count <= 0)
            {

            }

            var chapterCommentDto = _mapper.Map<List<GetChapterCommentDto>>(chapterComment);

            await _cache.SetValue(cacheKey, chapterCommentDto);

            return chapterCommentDto;
        }
    }
}
