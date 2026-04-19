namespace EduCore.API.Service.Email
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string SmtpServer { get; set; } = null!;
        public int SmtpPort { get; set; }
    }
}
