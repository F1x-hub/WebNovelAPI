namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public class GetChapterDto
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
