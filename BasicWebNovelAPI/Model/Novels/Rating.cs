using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels
{
    public class Rating
    {
        public int Id { get; set; }

        public double Value { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int NovelId { get; set; }
        public Novel Novel { get; set; } = null!;
    }
}
