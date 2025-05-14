namespace BasicWebNovelAPI.Model.Dto.User
{
    public class UpdateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdult { get; set; }

        public int RoleId { get; set; }
    }
}
