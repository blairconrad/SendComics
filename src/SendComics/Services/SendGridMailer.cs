namespace SendComics.Services
{
    using System;
    using System.Threading.Tasks;

    using SendGrid;
    using SendGrid.Helpers.Mail;

    internal class SendGridMailer
    {
        private readonly SendGridClient client =
            new SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));

        public Task<Response> SendEmailAsync(SendGridMessage message)
        {
            return this.client.SendEmailAsync(message);
        }
    }
}
