namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class CreateChapterDto
    {
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PdfPath { get; set; }
        public bool UsePdfContent { get; set; }
    }
}
