using Marketplace.Models.DATA;
using Newtonsoft.Json;

namespace Marketplace.Models.DTO
{
	public class ProductDto
	{
		public long Id { get; set; }
		public string? Name { get; set; }
		public bool Active { get; set; }
		public string? ShortDescription { get; set; }
		public string? ShortDescription2 { get; set; }
		public string? SeoLink { get; set; }
		public float NetPrice { get; set; }
		public float Vat { get; set; }
		public float Price { get; set; }
		public int Amount { get; set; }
		public int? OrderIndex { get; set; }
		public List<string> CoverImages { get; set; } = new List<string>();
		public List<ProductDto>? Variants { get; set; } = new List<ProductDto>();
		public List<ProductDto>? RelatedProducts { get; set; } = new List<ProductDto>();
		public List<ProductDto>? RelatedDesigns { get; set; } = new List<ProductDto>();



		public ProductDto()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productEntity"></param>
		public ProductDto(ProductDm productEntity)
		{
			if (productEntity != null)
			{
				Id = productEntity.Id;
				Name = productEntity.Name;
				Active = productEntity.Active;
				ShortDescription = productEntity.ShortDescription;
				ShortDescription2 = productEntity.ShortDescription2;
				SeoLink = productEntity.SeoLink;
				Name = productEntity.Name;
				Price = productEntity.Price;
				Vat = productEntity.Vat;
				NetPrice = productEntity.NetPrice;
				Amount = productEntity.Amount;
				OrderIndex = productEntity.OrderIndex;

				if (!string.IsNullOrWhiteSpace(productEntity.CoverImages))
				{
					try
					{
						CoverImages = JsonConvert.DeserializeObject<List<string>>(productEntity.CoverImages);
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal string GetCoverImages()
		{
			var coverImages = string.Empty;
			try
			{
				coverImages = JsonConvert.SerializeObject(CoverImages);
			}
			catch { }
			return coverImages;
		}
	}
}
