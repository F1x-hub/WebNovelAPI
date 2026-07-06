namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class GetNovelCommentDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikesCount { get; set; }
    }
}
