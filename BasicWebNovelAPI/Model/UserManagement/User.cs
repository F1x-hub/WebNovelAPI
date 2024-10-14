using System.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.UserManagement
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public string? TemporaryCode { get; set; } 
        public DateTime? CodeExpirationTime { get; set; }

        public string Phone { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
        public AuthIssuer AuthIssuer { get; set; }

        public ICollection<UserImages>? UserImages { get; set; }
        public ICollection<Novel> Novels { get; set; }

        public ICollection<UserLibrary> Library { get; set; }

        //sdasdas
    }
}
