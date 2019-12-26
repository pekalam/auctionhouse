using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Core.Common.Auth;
using Microsoft.Extensions.Logging;

namespace Web.Adapters
{
    public class ResetLinkSenderServiceSettings
    {
        public string AppResetUrl { get; set; }
        public string NoReplyEmail { get; set; }
        public string Subject { get; set; }
        public string SmtpServerAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ResetLinkSenderService : IResetLinkSenderService
    {
        private ResetLinkSenderServiceSettings _settings;
        private ILogger<ResetLinkSenderService> _logger;

        public ResetLinkSenderService(ResetLinkSenderServiceSettings settings, ILogger<ResetLinkSenderService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void SendResetLink(string resetId, string username, string email)
        {
            var link = $"{_settings.AppResetUrl}";

            MailMessage msg = new MailMessage(from: _settings.NoReplyEmail,
                to: email);

            msg.Subject = _settings.Subject;
            msg.Body = $"Your reset code: {resetId}\n Go to {link} and reset password.";

            SmtpClient client = new SmtpClient()
            {
                Port = 587,
                Host = _settings.SmtpServerAddress,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            try
            {
                _logger.LogDebug("Sending reset link to user: {username} with email: {email}", username, email);
                client.Send(msg);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Cannot send reset link to {username} with email: {email}", username, email);
                throw;
            }
        }
    }
}