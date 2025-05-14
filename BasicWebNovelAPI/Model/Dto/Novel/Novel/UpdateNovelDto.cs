using BasicWebNovelAPI.Enum;

namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class UpdateNovelDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Views { get; set; }
        public NovelStatus? Status { get; set; }
        public bool? IsAdultContent { get; set; }
    }
}
