using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Hubs;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentRepository(BasicWebNovelContext context, IDistributedCache cache, IMapper mapper, IHubContext<CommentHub> hubContext)
        {
            _context = context;
            _cache = cache;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<GetNovelCommentDto> SendNovelComment(CreateNovelCommentDto createNovelCommentDto, int userId, int novelId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User Not Found");


            var comment = _mapper.Map<NovelComments>(createNovelCommentDto);
            comment.UserId = userId;
            comment.NovelId = novelId;
            comment.DisplayName = user.UserName;

            comment.PublishedDate = DateTime.Now;

            if (comment.Content == "")
            {
                throw new Exception("You Need Fill Comments!");
            }

            await _context.NovelComments.AddAsync(comment);
            await _context.SaveChangesAsync();


            var novelCommentDto = _mapper.Map<GetNovelCommentDto>(comment);
            

            await _hubContext.Clients.All.SendAsync("ReceiveComment", novelCommentDto);

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
            comment.DisplayName = user.UserName;
            

            comment.PublishedDate = DateTime.Now;

            if (comment.Content == "")
            {
                throw new Exception("You Need Fill Comments!");
            }

            await _context.ChapterComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var chapterCommentDto = _mapper.Map<GetChapterCommentDto>(comment);

            return chapterCommentDto;
        }

        public async Task<List<GetNovelCommentDto>> GetAllCommentNovel(int novelId)
        {
           
            var novelComment = await _context.NovelComments
                .Include(nc => nc.Likes)  // Ensure likes are loaded
                .Where(n => n.NovelId == novelId)
                .ToListAsync();

            if (novelComment.Count <= 0)
            {
                throw new Exception("Not Have Comments");
            }

            var novelCommentDto = _mapper.Map<List<GetNovelCommentDto>>(novelComment);

            

            return novelCommentDto;
        }

        public async Task<List<GetChapterCommentDto>> GetAllCommentChapter(int chapterId)
        {
            
            var chapterComment = await _context.ChapterComments
                .Include(nc => nc.Likes)  // Ensure likes are loaded
                .Where(n => n.ChapterId == chapterId)
                .ToListAsync();

            if (chapterComment.Count <= 0)
            {
                throw new Exception("Not Have Comments");
            }

            var chapterCommentDto = _mapper.Map<List<GetChapterCommentDto>>(chapterComment);

            

            return chapterCommentDto;
        }

        public async Task<bool> DeleteNovelComments(int commentId, int novelId, int userId)
        {
            var novelComment = await _context.NovelComments
                .FirstOrDefaultAsync(n => n.Id == commentId && n.NovelId == novelId && n.UserId == userId);

            if (novelComment == null)
            {
                return false;
            }

            _context.NovelComments.Remove(novelComment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("CommentDeleted", commentId);

            return true;
        }

        public async Task<bool> DeleteChapterComments(int commentId, int chapterId, int userId)
        {
            var chapterComment = await _context.ChapterComments
                .FirstOrDefaultAsync(n => n.Id == commentId && n.ChapterId == chapterId && n.UserId == userId);

            if (chapterComment == null)
            {
                return false;
            }

            _context.ChapterComments.Remove(chapterComment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("CommentDeleted", commentId);

            return true;
        }
        
        
        public async Task<bool> ToggleNovelCommentLike(int commentId, int userId)
        {
            var existingLike = await _context.NovelCommentLikes
                .FirstOrDefaultAsync(l => l.NovelCommentId == commentId && l.UserId == userId);

            if (existingLike != null)
            {
                // Remove like if already exists
                _context.NovelCommentLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                // Add new like
                var newLike = new NovelCommentLikes
                {
                    NovelCommentId = commentId,
                    UserId = userId,
                    LikedDate = DateTime.Now
                };

                await _context.NovelCommentLikes.AddAsync(newLike);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        
        public async Task<bool> HasUserLikedNovelComment(int commentId, int userId)
        {
            // Проверяем, существует ли запись лайка для данного комментария и пользователя
            return await _context.NovelCommentLikes
                .AnyAsync(like => like.NovelCommentId == commentId && like.UserId == userId);
        }

        public async Task<bool> ToggleChapterCommentLike(int commentId, int userId)
        {
            var existingLike = await _context.ChapterCommentLikes
                .FirstOrDefaultAsync(l => l.ChapterCommentId == commentId && l.UserId == userId);

            if (existingLike != null)
            {
                // Remove like if already exists
                _context.ChapterCommentLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                // Add new like
                var newLike = new ChapterCommentLikes
                {
                    ChapterCommentId = commentId,
                    UserId = userId,
                    LikedDate = DateTime.Now
                };

                await _context.ChapterCommentLikes.AddAsync(newLike);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        
        public async Task<bool> HasUserLikedChapterComment(int commentId, int userId)
        {
            // Проверяем, существует ли запись лайка для данного комментария и пользователя
            return await _context.ChapterCommentLikes
                .AnyAsync(like => like.ChapterCommentId == commentId && like.UserId == userId);
        }

        public async Task<int> GetNovelCommentLikesCount(int commentId)
        {
            return await _context.NovelCommentLikes
                .CountAsync(l => l.NovelCommentId == commentId);
        }

        public async Task<int> GetChapterCommentLikesCount(int commentId)
        {
            return await _context.ChapterCommentLikes
                .CountAsync(l => l.ChapterCommentId == commentId);
        }
    }
}
