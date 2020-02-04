using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public class MailgunEmailService : IEmailService
    {
        public async Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message)
        {
			RestClient client = new RestClient();
			client.BaseUrl = new Uri("https://api.mailgun.net/v3");
			client.Authenticator = new HttpBasicAuthenticator("api", "YOUR_API_KEY");
			RestRequest request = new RestRequest();
			request.AddParameter("domain", "mg.jamesqmurphy.com", ParameterType.UrlSegment);
			request.Resource = "{domain}/messages";
			request.AddParameter("from", "Cold-Brewed DevOps <no-reply@jamesqmurphy.com>");
			request.AddParameter("to", emailAddress);
			request.AddParameter("subject", subject);
			request.AddParameter("text", message);
			request.Method = Method.POST;
			var restResponse = await client.ExecuteAsync(request);
			return new EmailResult
			{
				Success = restResponse.IsSuccessful,
				Details = restResponse.Content
			};
		}
	}
}
