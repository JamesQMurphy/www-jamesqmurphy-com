using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public class MailgunEmailService : IEmailService
    {
		public class Options
		{
			public string FromAddress { get; set; }
			public string MailDomain { get; set; } = "mg.jamesqmurphy.com";
			public string ServiceUrl { get; set; } = "https://api.mailgun.net/v3";
			public string ServiceApiKey { get; set; }
		}

		private readonly Options _options;

		public MailgunEmailService(Options options)
		{
			_options = options;
		}

		public async Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message)
        {
			RestClient client = new RestClient();
			client.BaseUrl = new Uri(_options.ServiceUrl);
			client.Authenticator = new HttpBasicAuthenticator("api", _options.ServiceApiKey);
			RestRequest request = new RestRequest();
			request.AddParameter("domain", _options.MailDomain, ParameterType.UrlSegment);
			request.Resource = "{domain}/messages";
			request.AddParameter("from", _options.FromAddress);
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
