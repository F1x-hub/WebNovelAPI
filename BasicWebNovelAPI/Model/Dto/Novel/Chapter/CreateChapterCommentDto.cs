namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class CreateChapterCommentDto
    {
        public required string DisplayName { get; set; }
        public required string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;
    }
}
