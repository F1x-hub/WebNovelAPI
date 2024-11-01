using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class Chapter
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int NovelId { get; set; }

        [JsonIgnore]
        public Novel Novel { get; set; }

        public ICollection<ChapterComments> ChapterComments { get; set; }
    }
}
