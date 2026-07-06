using BasicWebNovelAPI.Enum;

namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class CreateNovelDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime PublishedDate { get; set; }
        public NovelStatus Status { get; set; } = NovelStatus.InProgress;
        public bool IsAdultContent { get; set; } = false;

        public required List<int> GenreIds { get; set; }
    }
}
