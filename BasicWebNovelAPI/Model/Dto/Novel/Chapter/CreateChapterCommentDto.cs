namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class CreateChapterCommentDto
    {
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;
    }
}
