namespace BasicWebNovelAPI.Model.Dto.User
{
    public struct VerifyCodeDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string TemporaryCode { get; set; }
    }
}
