namespace BasicWebNovelAPI.Model.Dto.User
{
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public string Phone { get; set; }

        public int RoleId { get; set; } 
    }
}
