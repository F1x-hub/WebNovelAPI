namespace BasicWebNovelAPI.Model.Dto.Novel.Library
{
    public class GetUserLibraryDto
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public string NovelTitle { get; set; }
        public int LastReadChapter { get; set; }
        public bool AddedChapter { get; set; } = false;
    }
}
