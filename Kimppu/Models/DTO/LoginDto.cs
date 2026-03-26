namespace Marketplace.Models.DTO
{
	public class LoginDto
	{
		public string? Email { get; set; }
		public string? Password { get; set; }
		public string? PasswordAgain { get; set; }
		public string? Code { get; set; }
		public string? CodeToken { get; set; }
	}
}
