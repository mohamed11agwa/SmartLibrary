using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SmartLibrary.Web.Settings;
using System.Net;
using System.Net.Mail;

namespace SmartLibrary.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MailSettings _mailSettings;

        public EmailSender(IOptions<MailSettings> mailSettings, IWebHostEnvironment webHostEnvironment)
        {
            _mailSettings = mailSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage message = new MailMessage()
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(_webHostEnvironment.IsDevelopment()? "mohamed.agwa1717@gmail.com" : email);

            SmtpClient smtpClient = new SmtpClient(_mailSettings.Host)
            {
                Port = _mailSettings.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password)
            };
            await smtpClient.SendMailAsync(message);
            smtpClient.Dispose();
        }
    }
}
