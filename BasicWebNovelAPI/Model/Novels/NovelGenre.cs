namespace BasicWebNovelAPI.Model.Novels
{
    public class NovelGenre
    {
        public int NovelId { get; set; }
        public Novel Novel { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
