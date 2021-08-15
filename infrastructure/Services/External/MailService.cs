using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace infrastructure.Services.External
{
    public interface IMailService
    {
        Task SendRegistrationAsync(string code, string address, string username, string userId);
    }

    public class MailService : IMailService
    {
        private readonly ILogger<MailService> Logger;
        private readonly MailSenderConfig MailSenderConfig;

        private readonly EmailAddress Sender;

        private readonly SendGridClient SendGridClient;

        public MailService(ILogger<MailService> logger, IOptions<MailSenderConfig> options)
        {
            Logger = logger;
            MailSenderConfig = options.Value;
            Sender = new EmailAddress(MailSenderConfig.Email, MailSenderConfig.Name);
            SendGridClient = new SendGridClient(MailSenderConfig.ApiKey);
        }

        public async Task SendRegistrationAsync(string code, string address, string username, string userId)
        {
            Logger.Log(LogLevel.Information, $"Started sending registration for {address}.");
            var to = new EmailAddress(address, username);
            var url =
                $"{MailSenderConfig.ConfirmRegistrationBaseUrl}?userId={userId}&token={HttpUtility.UrlEncode(code)}";
            var msg = MailHelper.CreateSingleTemplateEmail(Sender, to, MailSenderConfig.TemplateId,
                new { registration_code = code, user_name = username, url });

            var response = await SendGridClient.SendEmailAsync(msg).ConfigureAwait(false);
            if (!IsSuccessStatusCode(response.StatusCode))
            {
                Logger.Log(LogLevel.Error, "Mail has not been sent.");
            }
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }
    }
}