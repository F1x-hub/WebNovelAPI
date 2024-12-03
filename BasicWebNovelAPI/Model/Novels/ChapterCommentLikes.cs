using System.Text.Json.Serialization;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels;

public class ChapterCommentLikes
{
    public int Id { get; set; }
    
   
    public DateTime LikedDate { get; set; }
    
    [JsonIgnore]
    public ChapterComments ChapterComment { get; set; }

    public int ChapterCommentId { get; set; }
    
    
    
    [JsonIgnore]
    public User User { get; set; }
    
    public int UserId { get; set; }
}