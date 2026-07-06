using BasicWebNovelAPI.Enum;

namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class GetNovelDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int Views { get; set; }
        public NovelStatus Status { get; set; }
        public string StatusString => Status.ToString();
        public bool IsAdultContent { get; set; }
        public int UserId { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public int TotalChapters { get; set; }
        public double AverageRating { get; set; }
    }
}
