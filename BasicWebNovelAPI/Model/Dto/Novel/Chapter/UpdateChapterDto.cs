namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class UpdateChapterDto
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string PdfPath { get; set; }
        public bool UsePdfContent { get; set; }
        public int ChapterNumber { get; set; }
    }
}
