using System;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.Coins
{
    public class CoinTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public CoinTransactionType Type { get; set; }
        public int Amount { get; set; }
        public int? RelatedChapterId { get; set; }
        public Chapter? RelatedChapter { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
