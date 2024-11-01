using BasicWebNovelAPI.Model.UserManagement;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.Novels
{
    public class ChapterComments
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }

        public string Content { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;


        
        public int ChapterId { get; set; }
        [JsonIgnore]
        public Chapter Chapter { get; set; }



        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        
        
    }
}
