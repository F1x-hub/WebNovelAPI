using System.Text.Json.Serialization;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels;

public class ChapterCommentLikes
{
    public int Id { get; set; }
    
   
    public DateTime LikedDate { get; set; }
    
    [JsonIgnore]
    public ChapterComments ChapterComment { get; set; } = null!;

    public int ChapterCommentId { get; set; }
    
    
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    public int UserId { get; set; }
}