namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public struct UserLibraryDto
    {
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public int LastReadChapter { get; set; } 
    }
}
