namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class GetChapterCommentDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public int ChapterId { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikesCount { get; set; }
    }
}
