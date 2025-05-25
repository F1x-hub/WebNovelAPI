namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class GetNovelCommentDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikesCount { get; set; }
    }
}
