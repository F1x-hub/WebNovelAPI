namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class CreateNovelCommentDto
    {
        public required string DisplayName { get; set; }
        public required string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;
    }
}
