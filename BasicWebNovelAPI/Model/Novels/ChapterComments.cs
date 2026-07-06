using BasicWebNovelAPI.Model.UserManagement;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class ChapterComments
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public int NovelId { get; set; }
        [JsonIgnore]
        [ForeignKey("NovelId")]
        public Novel Novel { get; set; } = null!;
        
        public int ChapterId { get; set; }
        [JsonIgnore]
        [ForeignKey("ChapterId")]
        public Chapter Chapter { get; set; } = null!;

        public int UserId { get; set; }
        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
       
        public ICollection<ChapterCommentLikes> Likes { get; set; } = new List<ChapterCommentLikes>();
    }
}
