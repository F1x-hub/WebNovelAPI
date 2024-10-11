using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class NovelImages
    {
        public int Id { get; set; }
        [NotMapped]
        [JsonIgnore]
        public Novel Novels { get; set; }
        public int NovelId { get; set; }
        public string ImageSource { get; set; }

    }
}
