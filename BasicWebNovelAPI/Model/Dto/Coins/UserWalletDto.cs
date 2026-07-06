using System.Collections.Generic;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class UserWalletDto
    {
        public int Balance { get; set; }
        public int TotalEarned { get; set; }
        public List<CoinTransactionDto> Transactions { get; set; } = new List<CoinTransactionDto>();
    }
}
