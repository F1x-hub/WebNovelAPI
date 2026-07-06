using System;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class ChapterAccessDto
    {
        public bool IsAccessible { get; set; }
        public bool IsFree { get; set; }
        public bool IsScheduleUnlocked { get; set; }
        public bool IsPurchased { get; set; }
        public int CoinPrice { get; set; }
        public DateTime? UnlocksAt { get; set; }
    }
}
