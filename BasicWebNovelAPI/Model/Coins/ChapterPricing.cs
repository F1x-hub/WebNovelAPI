using System;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.Coins
{
    public class ChapterPricing
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public Novel Novel { get; set; } = null!;
        public int FreeChaptersCount { get; set; }
        public int CoinPricePerChapter { get; set; }
        public bool UnlockScheduleEnabled { get; set; }
        public int UnlockIntervalDays { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
