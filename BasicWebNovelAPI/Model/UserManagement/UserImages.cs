using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BasicWebNovelAPI.Model.UserManagement
{
    public class UserImages
    {
        public int Id { get; set; }
        [NotMapped]
        [JsonIgnore]
        public User User { get; set; }
        public int UserId { get; set; }
        public string ImageSource { get; set; }
    }
}
