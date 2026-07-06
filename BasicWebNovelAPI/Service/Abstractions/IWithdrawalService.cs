using System.Collections.Generic;
using System.Threading.Tasks;
using BasicWebNovelAPI.Model.Dto.Coins;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IWithdrawalService
    {
        Task<WithdrawalDto> RequestWithdrawalAsync(int authorId, int coinsAmount);
        Task<IEnumerable<WithdrawalDto>> GetWithdrawalsAsync(int authorId);
    }
}
