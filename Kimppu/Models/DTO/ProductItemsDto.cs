namespace Marketplace.Controllers
{
	public class ProductItemsDto
	{
		public long ProductId { get; set; }
		public List<long> ItemIds { get; set; } = new List<long>();
	}
}