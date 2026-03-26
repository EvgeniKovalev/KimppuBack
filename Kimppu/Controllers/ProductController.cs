using Marketplace.Models.DTO;
using Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ProductController : ControllerBase
	{
		private ProductService _productService;

		public ProductController(ProductService productService)
		{
			_productService = productService;
		}

		[HttpPost("")]
		public async Task<IActionResult> GetProduct(FilterDto filter)
		{
			var product = await _productService.GetProduct(filter);
			return Ok(new ResponseDto() { IsSuccess = true, Model = product, Message = $"" });
		}

		[HttpPost("search")]
		public async Task<IActionResult> LoadProducts(FilterDto filter)
		{
			var products = await _productService.LoadProducts(filter);
			return Ok(new ResponseDto() { IsSuccess = true, Model = products, Message = $"" });
		}

		[AllowAnonymous]
		[HttpPost("productvariants")]
		public async Task<IActionResult> GetProductVariants([FromBody] long productId)
		{
			try
			{
				var productVariants = _productService.GetProductRelations(productId, ProductRelationType.VariantProduct);
				return Ok(new ResponseDto() { IsSuccess = true, Model = productVariants, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[AllowAnonymous]
		[HttpPost("relatedDesigns")]
		public async Task<IActionResult> GetProductRelatedDesigns([FromBody] long productId)
		{
			try
			{
				var productRelatedDesigns = _productService.GetProductRelations(productId, ProductRelationType.RelatedDesign);
				return Ok(new ResponseDto() { IsSuccess = true, Model = productRelatedDesigns, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[AllowAnonymous]
		[HttpPost("relatedproducts")]
		public async Task<IActionResult> GetRelatedProducts([FromBody] long productId)
		{
			try
			{
				var relatedProducts = _productService.GetProductRelations(productId, ProductRelationType.RelatedProduct);
				return Ok(new ResponseDto() { IsSuccess = true, Model = relatedProducts, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("saveproductvariants")]
		public async Task<IActionResult> SaveProductVariants(ProductItemsDto productItems)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.SaveRelatedProducts(currentUserID, productItems.ProductId, productItems.ItemIds, ProductRelationType.VariantProduct);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("saverelatedproducts")]
		public async Task<IActionResult> SaveRelatedProducts(ProductItemsDto productItems)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.SaveRelatedProducts(currentUserID, productItems.ProductId, productItems.ItemIds, ProductRelationType.RelatedProduct);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("saverelateddesigns")]
		public async Task<IActionResult> SaveRelatedDesigns(ProductItemsDto productItems)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.SaveRelatedProducts(currentUserID, productItems.ProductId, productItems.ItemIds, ProductRelationType.RelatedDesign);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("add")]
		public async Task<IActionResult> AddProduct(ProductDto product)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.AddProduct(currentUserID, product);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("edit")]
		public async Task<IActionResult> EditProduct(ProductDto product)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.EditProduct(currentUserID, product);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("clone")]
		public async Task<IActionResult> CloneProduct(ProductDto product)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.AddProduct(currentUserID, product);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}
		

		[Authorize]
		[HttpPost("remove")]
		public async Task<IActionResult> RemoveProduct([FromBody] long productId)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.RemoveProduct(currentUserID, productId);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("saveorder")]
		public async Task<IActionResult> SaveProductOrder(List<int> productIds)
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				await _productService.SaveProductOrder(currentUserID, productIds);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}
	}
}
