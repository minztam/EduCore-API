using EduCore.API.Repositories.ResponseMessage;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace EduCore.API.Service.Email
{
    public class EmailService
    {
        private readonly EmailSettings _email;
        private readonly ResponseMessageResult _respone;
        public EmailService(EmailSettings email, ResponseMessageResult respone)
        {
            _email = email;
            _respone = respone;
        }

        private async Task SendAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                _respone.SetFail("Email không được để trống");

            var mailMessage = new MailMessage(_email.SenderEmail, to, subject, body)
            {
                IsBodyHtml = true
            };

            using var client = new SmtpClient(_email.SmtpServer, _email.SmtpPort)
            {
                Credentials = new NetworkCredential(_email.SenderEmail, _email.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendWelcomeEmailAsync(string to, string tenDangNhap)
        {
            string subject = "Chào mừng đến với EduCore!";

            string body = $@"
<html>
<body style='font-family: Arial; background:#f4f4f4; padding:20px'>
    <table width='100%' style='max-width:600px;margin:auto;background:#fff;border-radius:10px;padding:20px'>
        <tr>
            <td style='text-align:center'>
                <h2 style='color:#4CAF50'>EduCore</h2>
            </td>
        </tr>
        <tr>
            <td>
                <h3>Xin chào {tenDangNhap},</h3>
                <p>Cảm ơn bạn đã đăng ký tài khoản.</p>
                <p>Chúc bạn học tập hiệu quả 🚀</p>
                <hr/>
                <small>Email tự động - vui lòng không trả lời</small>
            </td>
        </tr>
    </table>
</body>
</html>";

            await SendAsync(to, subject, body);
        }
    }
}
