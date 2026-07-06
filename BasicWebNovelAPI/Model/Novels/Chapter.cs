using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class Chapter
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PdfPath { get; set; } = string.Empty;
        public bool UsePdfContent { get; set; }
        public DateTime CreatedAt { get; set; }

        public int NovelId { get; set; }

        [JsonIgnore]
        public Novel Novel { get; set; } = null!;


        public ICollection<ChapterComments> ChapterComments { get; set; } = new List<ChapterComments>();

        public ICollection<UserChapterRead> UserChapterRead { get; set; } = new List<UserChapterRead>();
    }
}
