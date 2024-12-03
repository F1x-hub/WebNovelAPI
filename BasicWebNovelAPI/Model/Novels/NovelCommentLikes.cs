using System.Text.Json.Serialization;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Model.Novels;

public class NovelCommentLikes
{
    public int Id { get; set; }
    public DateTime LikedDate { get; set; }
    
    
    [JsonIgnore]
    public NovelComments NovelComment { get; set; }

    public int NovelCommentId { get; set; }
    
    
    
    [JsonIgnore]
    public User User { get; set; }
    
    public int UserId { get; set; }
}