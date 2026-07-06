namespace BasicWebNovelAPI.Model.Dto.User
{
    public class ChangePasswordDto
    {
        public int UserId { get; set; }
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
} 