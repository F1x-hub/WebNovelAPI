using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels
{
    public class Novel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        

        public DateTime PublishedDate { get; set; }
        
        public int Views { get; set; } = 0;
        public NovelStatus Status { get; set; } = NovelStatus.InProgress;
        public bool IsAdultContent { get; set; } = false;

        public int UserId { get; set; }
        public User User { get; set; } = null!;


        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<NovelGenre> NovelGenres { get; set; } = new List<NovelGenre>();

        public ICollection<NovelImages> NovelImages { get; set; } = new List<NovelImages>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<NovelView> NovelViews { get; set; } = new List<NovelView>();

        public ICollection<NovelComments> NovelComments { get; set; } = new List<NovelComments>();
        
    }
}
