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

        Task<bool> DeleteNovelComments(int commentId, int novelId, int userId);
        Task<bool> DeleteChapterComments(int commentId, int chapterId, int userId);
        Task<bool> ToggleNovelCommentLike(int commentId, int userId);
        Task<bool> ToggleChapterCommentLike(int commentId, int userId);
        Task<int> GetNovelCommentLikesCount(int commentId);
        Task<int> GetChapterCommentLikesCount(int commentId);
    }
}
