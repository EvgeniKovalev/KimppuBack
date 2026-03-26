namespace Marketplace.Models.DM
{
	public class ProductAttributeDm
	{
		public long Id { get; set; }
		public long ProductId { get; set; }
		public long TemplateId { get; set; }
		public long AttributeId { get; set; }
		public string? Value { get; set; }

		public ProductAttributeDm() { }
	}
}
