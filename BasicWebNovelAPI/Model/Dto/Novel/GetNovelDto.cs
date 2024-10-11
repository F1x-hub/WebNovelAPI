namespace BasicWebNovelAPI.Model.Dto.Novel
{
    public class GetNovelDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedDate { get; set; }
        public List<string> Genres { get; set; }

    }
}
