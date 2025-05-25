namespace BasicWebNovelAPI.Model.Dto.User
{
    public struct GetUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdult { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public int RoleId { get; set; }
        public string Role { get; set; }
        
        public bool HasNewChapters { get; set; }
    }
}
