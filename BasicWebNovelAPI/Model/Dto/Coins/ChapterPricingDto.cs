using System;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class ChapterPricingDto
    {
        public int NovelId { get; set; }
        public int FreeChaptersCount { get; set; }
        public int CoinPricePerChapter { get; set; }
        public bool UnlockScheduleEnabled { get; set; }
        public int UnlockIntervalDays { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
    }
}
