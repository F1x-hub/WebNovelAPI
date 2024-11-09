namespace BasicWebNovelAPI.Model.Dto.Novel.Chapter
{
    public class GetChapterCommentDto
    {
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        
    }
}
