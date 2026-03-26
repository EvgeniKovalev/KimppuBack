namespace Marketplace.Models.DTO
{
	public class FilterDto
	{
		public string? Name { get; set; }
		public string? SeoLink { get; set; }
		public bool? ActiveOnly { get; set; }

		public double? PriceMin { get; set; }
		public double? PriceMax { get; set; }

		public List<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();

		public FilterDto() { }
	}
}
