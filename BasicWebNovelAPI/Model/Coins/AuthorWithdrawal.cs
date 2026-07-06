using System;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Coins
{
    public class AuthorWithdrawal
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public int CoinsAmount { get; set; }
        public int PlatformFeeCoins { get; set; }
        public int NetCoins { get; set; }
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
