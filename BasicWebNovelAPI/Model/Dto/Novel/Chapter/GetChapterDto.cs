namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class GetChapterDto
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PdfPath { get; set; }
        public bool UsePdfContent { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}
