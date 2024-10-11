using System.Net;
using System.Net.Mail;
using BasicWebNovelAPI.Service.Abstractions;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class EmailRepository : IEmailRepository
    {
        private readonly IConfiguration _configuration;

        public EmailRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendToEmail(string email, string text)
        {
            var userName = _configuration["EmailSettings:UserName"];
            var password = _configuration["EmailSettings:Password"];


            using (var client = new SmtpClient(_configuration["EmailSettings:SmtpServer"]))
            {
                client.Port = int.Parse(_configuration["EmailSettings:Port"]);
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = true;


                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(_configuration["EmailSettings:From"]),
                    Subject = "Login Code",
                    Body = text,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                return "Email sent successfully";

            }


        }
    }
}
