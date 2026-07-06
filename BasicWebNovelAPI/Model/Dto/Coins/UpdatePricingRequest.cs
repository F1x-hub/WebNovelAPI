using System;

namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class UpdatePricingRequest
    {
        public int FreeChaptersCount { get; set; }
        public int CoinPricePerChapter { get; set; }
        public bool UnlockScheduleEnabled { get; set; }
        public int UnlockIntervalDays { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
    }
}
