namespace Marketplace.Models.DM
{
	public class UserDm
	{
		public long Id { get; set; }
		public string? Guid { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }
		public string? Code { get; set; }
		public DateTime Created { get; set; }
		public DateTime Removed { get; set; }
	}
}
