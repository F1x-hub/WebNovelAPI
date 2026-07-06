using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.UserManagement
{
    public class UserLibrary
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int NovelId { get; set; }
        public Novel Novel { get; set; } = null!;

        public int LastReadChapter { get; set; }
        public bool AddedChapter { get; set; } = false;
    }
}
