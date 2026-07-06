using System;
using System.Threading.Tasks;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Coins;
using BasicWebNovelAPI.Model.Dto.Coins;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ChapterAccessService : IChapterAccessService
    {
        private readonly BasicWebNovelContext _context;

        public ChapterAccessService(BasicWebNovelContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAccessChapterAsync(int userId, int chapterId)
        {
            var status = await GetChapterAccessStatusAsync(userId, chapterId);
            return status.IsAccessible;
        }

        public async Task<ChapterAccessDto> GetChapterAccessStatusAsync(int userId, int chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null)
            {
                throw new NotFoundException("Chapter not found");
            }

            // Check if user is the author or admin
            bool isAuthorOrAdmin = false;
            if (userId > 0)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    if (user.Role?.RoleName == "Admin" || chapter.Novel.UserId == userId)
                    {
                        isAuthorOrAdmin = true;
                    }
                }
            }

            var pricing = await _context.ChapterPricings
                .FirstOrDefaultAsync(p => p.NovelId == chapter.NovelId);

            // Defaults
            int freeChaptersCount = 10;
            int coinPrice = 1;
            bool scheduleEnabled = false;
            int intervalDays = 7;
            DateTime? startDate = null;

            if (pricing != null)
            {
                freeChaptersCount = pricing.FreeChaptersCount;
                coinPrice = pricing.CoinPricePerChapter;
                scheduleEnabled = pricing.UnlockScheduleEnabled;
                intervalDays = pricing.UnlockIntervalDays;
                startDate = pricing.ScheduleStartDate;
            }

            bool isFree = chapter.ChapterNumber <= freeChaptersCount;
            bool isScheduleUnlocked = false;
            DateTime? unlocksAt = null;

            if (!isFree && scheduleEnabled && startDate.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now >= startDate.Value)
                {
                    var daysSinceStart = (now - startDate.Value).TotalDays;
                    var opened = (int)Math.Floor(daysSinceStart / intervalDays);
                    if (chapter.ChapterNumber <= freeChaptersCount + opened)
                    {
                        isScheduleUnlocked = true;
                    }
                }

                if (!isScheduleUnlocked)
                {
                    var chaptersNeeded = chapter.ChapterNumber - freeChaptersCount;
                    unlocksAt = startDate.Value.AddDays(chaptersNeeded * intervalDays);
                }
            }

            bool isPurchased = false;
            if (userId > 0 && !isFree && !isScheduleUnlocked)
            {
                isPurchased = await _context.UserChapterUnlocks
                    .AnyAsync(u => u.UserId == userId && u.ChapterId == chapterId);
            }

            bool isAccessible = isAuthorOrAdmin || isFree || isScheduleUnlocked || isPurchased;

            return new ChapterAccessDto
            {
                IsAccessible = isAccessible,
                IsFree = isFree,
                IsScheduleUnlocked = isScheduleUnlocked,
                IsPurchased = isPurchased,
                CoinPrice = isFree ? 0 : coinPrice,
                UnlocksAt = unlocksAt
            };
        }
    }
}
