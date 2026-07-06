using System.Threading.Tasks;
using BasicWebNovelAPI.Model.Dto.Coins;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IChapterAccessService
    {
        Task<bool> CanAccessChapterAsync(int userId, int chapterId);
        Task<ChapterAccessDto> GetChapterAccessStatusAsync(int userId, int chapterId);
    }
}
