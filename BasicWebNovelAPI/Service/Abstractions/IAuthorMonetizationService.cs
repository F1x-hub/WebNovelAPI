using System.Threading.Tasks;
using BasicWebNovelAPI.Model.Dto.Coins;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IAuthorMonetizationService
    {
        Task<ChapterPricingDto> GetPricingAsync(int novelId);
        Task<ChapterPricingDto> SavePricingAsync(int authorId, int novelId, UpdatePricingRequest request);
    }
}
