using BasicWebNovelAPI.Enum;

namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class CreateNovelDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedDate { get; set; }
        public NovelStatus Status { get; set; } = NovelStatus.InProgress;
        public bool IsAdultContent { get; set; } = false;

        public List<int> GenreIds { get; set; }
    }
}
