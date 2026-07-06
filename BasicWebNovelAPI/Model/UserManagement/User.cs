using System.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Model.Novels;

namespace BasicWebNovelAPI.Model.UserManagement
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsAdult { get; set; } = false;

        public string? TemporaryCode { get; set; } 
        public DateTime? CodeExpirationTime { get; set; }

        public int FailedLoginAttempts { get; set; } = 0; 
        public DateTime? LockoutExpirationTime { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public AuthIssuer AuthIssuer { get; set; }

        public ICollection<UserImages> UserImages { get; set; } = new List<UserImages>();
        public ICollection<Novel> Novels { get; set; } = new List<Novel>();

        public ICollection<UserLibrary> Library { get; set; } = new List<UserLibrary>();
        public ICollection<NovelComments> NovelComments { get; set; } = new List<NovelComments>();
        public ICollection<ChapterComments> ChapterComments { get; set; } = new List<ChapterComments>();
        public ICollection<UserChapterRead> UserChapterRead { get; set; } = new List<UserChapterRead>();
        
        public bool HasNewChapters => Library?.Any(item => item.AddedChapter) ?? false;
    }
}
