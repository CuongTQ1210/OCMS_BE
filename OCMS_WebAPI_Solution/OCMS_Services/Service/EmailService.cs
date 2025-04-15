using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace OCMS_Services.Service
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPass)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        public async Task SendEmailCertificateAsync(string to, string subject, string body, byte[] attachment = null, string attachmentFileName = null)
        {
            if (!IsValidEmail(to))
                throw new ArgumentException("Invalid recipient email address format.");

            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    client.EnableSsl = true;
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpUser),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(to);

                    if (attachment != null && !string.IsNullOrEmpty(attachmentFileName))
                    {
                        using (var stream = new MemoryStream(attachment))
                        {
                            var attachmentContent = new Attachment(stream, attachmentFileName, "application/pdf");
                            mailMessage.Attachments.Add(attachmentContent);
                        }
                    }

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(
                    $"SMTP error: StatusCode={ex.StatusCode}, Message={ex.Message}, " +
                    $"Server={_smtpServer}, Port={_smtpPort}, User={_smtpUser}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to send email to {to}. Server={_smtpServer}, Port={_smtpPort}", ex);
            }
        }

        public async Task SendCertificateEmailAsync(string to, string subject, string body, byte[] attachment, string attachmentFileName)
        {
            // Delegate to SendEmailAsync with validation for certificate-specific requirements
            if (attachment == null || attachment.Length == 0)
                throw new ArgumentException("Certificate attachment cannot be null or empty.");
            if (string.IsNullOrEmpty(attachmentFileName) || !attachmentFileName.EndsWith(".pdf"))
                throw new ArgumentException("Attachment filename must be a valid PDF filename.");

            await SendEmailCertificateAsync(to, subject, body, attachment, attachmentFileName);
        }
    }
}
