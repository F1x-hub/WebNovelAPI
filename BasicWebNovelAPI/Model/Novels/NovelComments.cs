using BasicWebNovelAPI.Model.UserManagement;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class NovelComments
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;

        [JsonIgnore]
        public Novel Novel { get; set; }
        public int NovelId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int UserId { get; set; }
    }
}
