namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public class CreateChapterDto
    {
        public int ChapterNumber { get; set; } 
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
