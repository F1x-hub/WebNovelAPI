using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels
{
    public class UserChapterRead
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        public bool IsRead { get; set; } = false;
    }
}
