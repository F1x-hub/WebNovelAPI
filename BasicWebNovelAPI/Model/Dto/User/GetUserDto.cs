namespace BasicWebNovelAPI.Model.Dto.User
{
    public struct GetUserDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }

        public int RoleId { get; set; }
        public string Role { get; set; }
    }
}
