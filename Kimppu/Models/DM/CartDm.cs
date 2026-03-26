namespace Marketplace.Models.DM
{
	public class CartDm
	{
		public long Id { get; set; }
		public string? VisitorToken { get; set; }
		public int ProductCount { get; set; }
		public DateTime Created { get; set; }
	}
}
