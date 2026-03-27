using Dapper;
using Marketplace.Models.DATA;
using Marketplace.Models.DTO;
using Marketplace.Repository;

namespace Marketplace.Services
{
	public class ProductService
	{
		private readonly RepositorySettings _repositorySettings;
		private readonly AuthService _authService;

		private int _cacheIntervalSec = 60;
		private long _lastUpdateTicks;
		private IEnumerable<ProductDm> _allProducts;
		private IEnumerable<ProductRelationDm> _allProductRelations;

		public ProductService(RepositorySettings repositorySettings, AuthService authService)
		{
			_repositorySettings = repositorySettings;
			_authService = authService;
		}

		private async Task CheckCache(bool force = false)
		{
			if (DateTime.UtcNow.AddSeconds(-_cacheIntervalSec).Ticks > _lastUpdateTicks || force)
			{
				var productConnection = _repositorySettings.CreateProductConnection();
				_allProducts = await productConnection.QueryAsync<ProductDm>("Select * from product where removed is null order by OrderIndex");
				_allProductRelations = await productConnection.QueryAsync<ProductRelationDm>("Select * from productRelation");
				_lastUpdateTicks = DateTime.UtcNow.Ticks;
			}
		}

		internal async Task<IEnumerable<ProductDto>> LoadProducts(FilterDto filter)
		{
			await CheckCache();
			var products = new List<ProductDto>();
			if (filter != null)
			{
				if (filter.ActiveOnly == true)
				{
					foreach (var productEntity in _allProducts.Where(p => p.Active == true))
					{
						var product = new ProductDto(productEntity);
						product.Variants = GetProductRelations(product.Id, ProductRelationType.VariantProduct);
						product.RelatedProducts = GetProductRelations(product.Id, ProductRelationType.RelatedProduct);
						product.RelatedDesigns = GetProductRelations(product.Id, ProductRelationType.RelatedDesign);
						products.Add(product);
					}
				}
				else
				{
					foreach (var productEntity in _allProducts)
					{
						var product = new ProductDto(productEntity);
						product.Variants = GetProductRelations(product.Id, ProductRelationType.VariantProduct);
						product.RelatedProducts = GetProductRelations(product.Id, ProductRelationType.RelatedProduct);
						product.RelatedDesigns = GetProductRelations(product.Id, ProductRelationType.RelatedDesign);
						products.Add(product);
					}
				}
			}
			else
			{
				foreach (var productEntity in _allProducts)
				{
					var product = new ProductDto(productEntity);
					product.Variants = GetProductRelations(product.Id, ProductRelationType.VariantProduct);
					product.RelatedProducts = GetProductRelations(product.Id, ProductRelationType.RelatedProduct);
					product.RelatedDesigns = GetProductRelations(product.Id, ProductRelationType.RelatedDesign);
					products.Add(product);
				}
			}
			return products;
		}

		internal async Task<ProductDto?> GetProduct(FilterDto filter)
		{
			await CheckCache();

			ProductDto? product = null;
			if (filter != null && !string.IsNullOrEmpty(filter.SeoLink))
			{
				var productEntity = _allProducts.FirstOrDefault(p => p.SeoLink == filter.SeoLink);
				if (productEntity != null)
				{
					product = new ProductDto(productEntity);
					product.Variants = GetProductRelations(product.Id, ProductRelationType.VariantProduct);
					product.RelatedProducts = GetProductRelations(product.Id, ProductRelationType.RelatedProduct);
					product.RelatedDesigns = GetProductRelations(product.Id, ProductRelationType.RelatedDesign);
				}
			}
			return product;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserID"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		internal async Task AddProduct(string? currentUserID, ProductDto product)
		{
			var userHasRight = await _authService.UserHasRights(currentUserID, new List<AccessRightEnum>() { AccessRightEnum.PRODUCTS });
			if (userHasRight && product != null)
			{
				var productConnection = _repositorySettings.CreateProductConnection();
				var newProductParams = new
				{
					name = product.Name,
					seoLink = product.SeoLink,
					shortDescription = product.ShortDescription,
					shortDescription2 = product.ShortDescription2,
					coverImages = product.GetCoverImages(),
					netPrice = product.NetPrice,
					vat = product.Vat,
					price = product.Price
				};
				await productConnection.ExecuteAsync(@"insert into [product] (name, seoLink, shortDescription,shortDescription2, coverImages, netPrice, vat, price, active) 
						values (@name, @seoLink, @shortDescription, @shortDescription2, @coverImages, @netPrice, @vat, @price, 0)", newProductParams);

				await CheckCache(true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserID"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		internal async Task EditProduct(string? currentUserID, ProductDto product)
		{
			var userHasRight = await _authService.UserHasRights(currentUserID, new List<AccessRightEnum>() { AccessRightEnum.PRODUCTS });
			if (userHasRight && product != null)
			{
				var productEntity = _allProducts.FirstOrDefault(p => p.Id == product.Id);
				if (productEntity != null)
				{
					var productConnection = _repositorySettings.CreateProductConnection();
					var productParams = new
					{
						id = product.Id,
						name = product.Name,
						seoLink = product.SeoLink,
						shortDescription = product.ShortDescription,
						shortDescription2 = product.ShortDescription2,
						coverImages = product.GetCoverImages(),
						netPrice = product.NetPrice,
						vat = product.Vat,
						price = product.Price,
						active = product.Active
					};
					await productConnection.ExecuteAsync(@"update [product] set name=@name, seoLink=@seoLink, shortDescription=@shortDescription, shortDescription2=@shortDescription2,
						coverImages=@coverImages, netPrice=@netPrice, vat=@vat, price=@price, active=@active where id = @id", productParams);

					await CheckCache(true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserID"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		internal async Task RemoveProduct(string? currentUserID, long productId)
		{
			var userHasRight = await _authService.UserHasRights(currentUserID, new List<AccessRightEnum>() { AccessRightEnum.PRODUCTS });
			if (userHasRight)
			{
				var productEntity = _allProducts.FirstOrDefault(p => p.Id == productId);
				if (productEntity != null)
				{
					var productConnection = _repositorySettings.CreateProductConnection();
					await productConnection.ExecuteAsync("update product set removed = @removed where id = @productId", new { productId = productId, removed = DateTime.UtcNow });

					await CheckCache(true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserID"></param>
		/// <param name="productIds"></param>
		/// <returns></returns>
		internal async Task SaveProductOrder(string? currentUserID, List<int> productIds)
		{
			var userHasRight = await _authService.UserHasRights(currentUserID, new List<AccessRightEnum>() { AccessRightEnum.PRODUCTS });
			if (userHasRight && _allProducts != null && productIds != null && productIds.Count > 0)
			{
				_allProducts.ToList().ForEach(p => p.OrderIndex = 0);
				for (int i = 0; i < productIds.Count; i++)
				{
					var product = _allProducts.FirstOrDefault(p => p.Id == productIds[i]);
					if (product != null)
					{
						product.OrderIndex = i + 1;
					}
				}

				var productConnection = _repositorySettings.CreateProductConnection();
				await productConnection.ExecuteAsync(@"update product set OrderIndex = @OrderIndex where Id = @Id", _allProducts);
				await CheckCache(true);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserID"></param>
		/// <param name="productId"></param>
		/// <param name="relatedProductIds"></param>
		/// <param name="relationType"></param>
		/// <returns></returns>
		internal async Task SaveRelatedProducts(string? currentUserID, long productId, List<long> relatedProductIds, ProductRelationType relationType)
		{
			var userHasRight = await _authService.UserHasRights(currentUserID, new List<AccessRightEnum>() { AccessRightEnum.PRODUCTS });
			if (userHasRight && _allProducts != null && productId > 0 && relatedProductIds.Count > 0)
			{
				var selectedRelatedProducts = _allProducts.Where(p => relatedProductIds.Contains(p.Id)).ToList();

				var productConnection = _repositorySettings.CreateProductConnection();
				await productConnection.ExecuteAsync(@"delete productRelation where productId = @Id and relationType = @relationType", new { id = productId, relationType = relationType });
				await productConnection.ExecuteAsync(@$"insert into productRelation (productId, relationType, relatedProductId) values ({productId}, {(int)relationType}, @id)", selectedRelatedProducts);
				await CheckCache(true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="relationType"></param>
		/// <returns></returns>
		internal List<ProductDto>? GetProductRelations(long productId, ProductRelationType relationType)
		{
			var productVariants = new List<ProductDto>();
			if (productId > 0)
			{
				var variantIds = _allProductRelations.Where(v => v.ProductId == productId && v.RelationType == (int)relationType).Select(v => v.RelatedProductId).ToList();
				var crossVariantIds = new List<long>();
				if (relationType != ProductRelationType.RelatedProduct)
				{
					crossVariantIds = _allProductRelations.Where(v => v.RelatedProductId == productId && v.RelationType == (int)relationType).Select(v => v.ProductId).ToList();
				}

				var variantEntities = _allProducts.Where(p => variantIds.Contains(p.Id) || crossVariantIds.Contains(p.Id)).Distinct().ToList();
				foreach (var variantEntity in variantEntities)
				{
					productVariants.Add(new ProductDto(variantEntity));
				}
			}
			return productVariants;
		}
	}
}
