namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public class UpdateNovelDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedDate { get; set; }
        public List<UpdateChapterDto> Chapters { get; set; }
    }
}
