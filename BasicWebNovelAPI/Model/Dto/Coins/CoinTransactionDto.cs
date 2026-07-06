using System;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class CoinTransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int? RelatedChapterId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
