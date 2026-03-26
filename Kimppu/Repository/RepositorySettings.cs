using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Marketplace.Repository
{
	public class RepositorySettings
	{
		private readonly string? _emailUsername;
		private readonly string? _emailKey;

		private readonly string? _backendAddress;
		private readonly string _jwtAudience;
		private readonly string _jwtAudienceIssuer;
		private readonly byte[] _jwtKey;

		private readonly string _productConnectionString;
		private readonly string _templateConnectionString;
		private readonly string _cartConnectionString;
		private readonly string _userConnectionString;

		public RepositorySettings(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
		{
			var azureSettings = configuration.GetSection("AzureSettings");
			var jwtSettings = configuration.GetSection("JwtSetting");

			_emailUsername = Environment.GetEnvironmentVariable("EmailUsername") ?? azureSettings["EmailUsername"];
			_emailKey = Environment.GetEnvironmentVariable("EmailKey") ?? azureSettings["EmailKey"];

			_jwtAudienceIssuer = "Marketplace infrastructure";
			_jwtAudience = Environment.GetEnvironmentVariable("FrontendAddress") ?? azureSettings["FrontendAddress"];
			_jwtKey = Encoding.UTF8.GetBytes(jwtSettings.GetSection("SecurityKey").Value!);

			_productConnectionString = Environment.GetEnvironmentVariable("ProductDataConnStr") ?? configuration.GetConnectionString("ProductDataConnStr");
			_templateConnectionString = Environment.GetEnvironmentVariable("TemplateConnStr") ?? configuration.GetConnectionString("TemplateConnStr");
			_cartConnectionString = Environment.GetEnvironmentVariable("CartDataConnStr") ?? configuration.GetConnectionString("CartDataConnStr");
			_userConnectionString = Environment.GetEnvironmentVariable("UserDataConnStr") ?? configuration.GetConnectionString("UserDataConnStr");

			var request = httpContextAccessor.HttpContext.Request;
			_backendAddress = $"{request.Scheme}://{request.Host}";
		}

		public IDbConnection CreateProductConnection()
		=> new SqlConnection(_productConnectionString);

		public IDbConnection CreateTemplateConnection()
=> new SqlConnection(_templateConnectionString);

		public IDbConnection CreateCartConnection()
=> new SqlConnection(_cartConnectionString);

		public IDbConnection CreateUserConnection()
=> new SqlConnection(_userConnectionString);

		public string? GetBackendAddress()
		{
			return _backendAddress;
		}
		public byte[] GetJwtKey()
		{
			return _jwtKey;
		}

		public string GetAudience()
		{
			return _jwtAudience;
		}

		public string GetAudienceIssuer()
		{
			return _jwtAudienceIssuer;
		}

		public async Task SendEmail(string senderNameTopic, string recipientAddress, string subject, string plainText, string html)
		{
			try
			{
				using (var client = new SmtpClient())
				{
					client.Host = "smtp.gmail.com";
					client.Port = 587;
					client.DeliveryMethod = SmtpDeliveryMethod.Network;
					client.UseDefaultCredentials = false;
					client.EnableSsl = true;
					client.Credentials = new NetworkCredential(_emailUsername, _emailKey);
					using (var message = new MailMessage(
							from: new MailAddress("hello@kenobo.com", senderNameTopic),
							to: new MailAddress(recipientAddress)
							))
					{

						message.Subject = subject;
						if (!string.IsNullOrWhiteSpace(html))
						{
							message.IsBodyHtml = true;
							message.Body = html;
						}
						else
						{
							message.Body = plainText;
						}

						await client.SendMailAsync(message);
					}
				}
			}
			catch (Exception ex)
			{
				int p = 0;
			}
		}
	}
}
