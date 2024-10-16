namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public class GetUserLibraryDto
    {
        public int NovelId { get; set; }
        public string NovelTitle { get; set; }
        public int LastReadChapter { get; set; }
    }
}
