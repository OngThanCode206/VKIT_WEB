using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public void SendEmail(string subject, string body)
    {
        var settings = _config.GetSection("EmailSettings");

        using (var client = new SmtpClient(settings["SmtpServer"], int.Parse(settings["SmtpPort"])))
        {
            // SỬA LỖI: Bắt buộc phải tắt UseDefaultCredentials trước khi gán tài khoản
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(settings["SenderEmail"], settings["SenderPassword"]);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            var mailMessage = new MailMessage(settings["SenderEmail"], settings["AdminEmail"])
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            client.Send(mailMessage);
        }
    }
}