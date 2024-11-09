namespace BasicWebNovelAPI.Model.Dto.Novel.Novel
{
    public class GetNovelCommentDto
    {
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } 
    }
}
