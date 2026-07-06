using System.Collections.Generic;
using System.Threading.Tasks;
using BasicWebNovelAPI.Model.Dto.Coins;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface ICoinService
    {
        Task<UserWalletDto> GetWalletAsync(int userId);
        Task<PaymentIntentDto> CreatePurchaseIntentAsync(int userId, int coinPackageId, int? customAmount);
        Task ConfirmPurchaseAsync(string stripePaymentIntentId);
        Task<bool> SpendCoinsAsync(int userId, int chapterId);
        Task<IEnumerable<CoinTransactionDto>> GetTransactionsAsync(int userId);
    }
}
