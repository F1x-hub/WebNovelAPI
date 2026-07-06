using BasicWebNovelAPI.Model.UserManagement;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class NovelComments
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [JsonIgnore]
        public Novel Novel { get; set; } = null!;
        public int NovelId { get; set; }
        [JsonIgnore]
        public User User { get; set; } = null!;
        public int UserId { get; set; }


        public ICollection<NovelCommentLikes> Likes { get; set; } = new List<NovelCommentLikes>();
    }
}
