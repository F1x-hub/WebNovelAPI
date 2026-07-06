using System;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.Coins
{
    public class UserChapterUnlock
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
