using System;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class WithdrawalDto
    {
        public int Id { get; set; }
        public int CoinsAmount { get; set; }
        public int PlatformFeeCoins { get; set; }
        public int NetCoins { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
