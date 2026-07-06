namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class GetChapterCommentDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public int ChapterId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikesCount { get; set; }
    }
}
