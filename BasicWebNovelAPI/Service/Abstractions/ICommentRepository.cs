using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Dto.Novel.Novel;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface ICommentRepository
    {
        Task<GetNovelCommentDto> SendNovelComment(CreateNovelCommentDto createNovelCommentDto, int userId, int novelId);
        Task<GetChapterCommentDto> SendChapterComment(CreateChapterCommentDto createChapterCommentDto, int userId, int chapterId);
        Task<List<GetNovelCommentDto>> GetAllCommentNovel(int novelId);
        Task<List<GetChapterCommentDto>> GetAllCommentChapter(int chapterId);

    }
}
