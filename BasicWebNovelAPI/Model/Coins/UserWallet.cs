using System;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Coins
{
    public class UserWallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int Balance { get; set; }
        public int TotalEarned { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
