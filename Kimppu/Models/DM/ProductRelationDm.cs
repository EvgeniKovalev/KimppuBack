namespace Marketplace.Models.DTO
{
	public class ProductRelationDm
	{
		public long Id { get; set; }
		public long ProductId { get; set; }
		public long RelatedProductId { get; set; }
		public int RelationType { get; set; }
	}
}
