namespace BasicWebNovelAPI.Model.Novels
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<NovelGenre> NovelGenres { get; set; }
    }
}
