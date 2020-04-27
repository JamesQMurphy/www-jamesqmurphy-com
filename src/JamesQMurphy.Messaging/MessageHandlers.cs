using JamesQMurphy.Email;
using System;
using System.Text.Json;

namespace JamesQMurphy.Messaging
{
    public static class MessageHandlers
    {
        public static void RegisterAllHandlers(DefaultMessageReceiver defaultMessageReceiver)
        {
            defaultMessageReceiver.AddMessageHandler(MessageTypes.SendEmail, HandleEmailMessage);
        }


        public static void HandleEmailMessage(Message message, IServiceProvider serviceProvider)
        {
            var emailService = (IEmailService) serviceProvider.GetService(typeof(IEmailService));
            var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message.Data);
            _ = emailService.SendEmailAsync(emailMessage).GetAwaiter().GetResult();
        }
    }
}
