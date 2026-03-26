namespace Marketplace.Models.DATA
{
	public class ProductDm
	{
		public long Id { get; set; }
		public string? Name { get; set; }
		public bool Active { get; set; }
		public string? SeoLink { get; set; }
		public string? ShortDescription { get; set; }
		public string? ShortDescription2 { get; set; }
		public string? CoverImages { get; set; }
		public float NetPrice { get; set; }
		public float Vat { get; set; }
		public float Price { get; set; }
		public int Amount { get; set; }
		public int? OrderIndex { get; set; }
		public DateTime? Removed { get; set; }
	}
}
