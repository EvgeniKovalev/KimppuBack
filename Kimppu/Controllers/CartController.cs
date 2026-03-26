using Marketplace.Models.DTO;
using Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class CartController : ControllerBase
	{
		private CartService _cartService;

		public CartController(CartService cartService)
		{
			_cartService = cartService;
		}

		[HttpGet("")]
		public async Task<IActionResult> GetCart()
		{
			var response = new ResponseDto() { IsSuccess = false };
			var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
			if (currentUserID != null)
			{
				response.Model = await _cartService.LoadCart(currentUserID);
				response.IsSuccess = true;
			}
			return Ok(response);
		}

		[HttpGet("productCount")]
		public async Task<IActionResult> GetProductCount()
		{
			var response = new ResponseDto() { IsSuccess = false };
			var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
			if (currentUserID != null)
			{
				response.Model = await _cartService.getProductCount(currentUserID);
				response.IsSuccess = true;
			}
			return Ok(response);
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddProduct(CartProductUpdate cartProductUpdate)
		{
			var response = new ResponseDto() { IsSuccess = false };
			var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
			if (currentUserID != null)
			{
				response.Model = await _cartService.AddProduct(currentUserID, cartProductUpdate.ProductID, cartProductUpdate.Amount);
				response.IsSuccess = true;
			}
			return Ok(response);
		}

		[HttpPost("remove")]
		public async Task<IActionResult> RemoveProduct(CartProductUpdate cartProductUpdate)
		{
			var response = new ResponseDto() { IsSuccess = false };
			var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
			if (currentUserID != null)
			{
				response.Model = await _cartService.RemoveProduct(currentUserID, cartProductUpdate.ProductID, cartProductUpdate.Amount);
				response.IsSuccess = true;
			}
			return Ok(response);
		}
	}
}
