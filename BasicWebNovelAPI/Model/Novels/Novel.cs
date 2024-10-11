using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels
{
    public class Novel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        

        public DateTime PublishedDate { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }


        public ICollection<Chapter>? Chapters { get; set; }
        public ICollection<NovelGenre> NovelGenres { get; set; }

        public ICollection<NovelImages>? NovelImages { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
