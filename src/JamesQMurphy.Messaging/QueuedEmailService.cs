using JamesQMurphy.Email;
using System.Text.Json;
using System.Threading.Tasks;

namespace JamesQMurphy.Messaging
{
    public class QueuedEmailService : IEmailService
    {
        private readonly IMessagePublisher _messagePublisher;
        public QueuedEmailService(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task<EmailResult> SendEmailAsync(EmailMessage emailMessage)
        {
            await _messagePublisher.PublishMessage(new Message(MessageTypes.SendEmail, JsonSerializer.Serialize(emailMessage)));
            return new EmailResult { Success = true, Details = "Email queued" };
        }
    }
}
