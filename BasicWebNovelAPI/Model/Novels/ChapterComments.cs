using BasicWebNovelAPI.Model.UserManagement;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class ChapterComments
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public int NovelId { get; set; }
        [JsonIgnore]
        [ForeignKey("NovelId")]
        public Novel Novel { get; set; }
        
        public int ChapterId { get; set; }
        [JsonIgnore]
        [ForeignKey("ChapterId")]
        public Chapter Chapter { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; }
       
        public ICollection<ChapterCommentLikes> Likes { get; set; }
    }
}
