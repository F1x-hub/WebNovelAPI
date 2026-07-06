namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class GetChapterDto
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PdfPath { get; set; } = string.Empty;
        public bool UsePdfContent { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public bool IsAccessible { get; set; }
        public bool IsFree { get; set; }
        public int CoinPrice { get; set; }
    }
}
