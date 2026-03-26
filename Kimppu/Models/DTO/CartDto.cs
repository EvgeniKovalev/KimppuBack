using Marketplace.Models.DATA;

namespace Marketplace.Models.DTO
{
	public class CartDto
	{
		public string? VisitorToken { get; set; }
		public List<ProductDto>? Products { get; set; }

		public float? Price { get; set; }
		public float? Vat { get; set; }
		public float? TotalPrice { get; set; }

		internal void AdoptProducts(IEnumerable<ProductDm> prooductEntities)
		{
			if (prooductEntities != null)
			{
				Products = new List<ProductDto>();
				foreach (var productEntity in prooductEntities)
				{
					Products.Add(new ProductDto(productEntity));
				}
			}
		}
	}
}
