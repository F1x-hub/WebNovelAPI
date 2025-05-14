using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels
{
    public class NovelView
    {
        public int Id { get; set; }
        
        public int NovelId { get; set; }
        public Novel Novel { get; set; }
        
        public int? UserId { get; set; }
        public User? User { get; set; }
        
        public string? IpAddress { get; set; }
        
        public DateTime ViewedAt { get; set; } = DateTime.Now;
    }
} 