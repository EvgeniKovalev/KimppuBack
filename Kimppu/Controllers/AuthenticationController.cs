using Marketplace.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private AuthService _authService;

		public AuthenticationController(AuthService authService)
		{
			_authService = authService;
		}

		[HttpGet("version")]
		public async Task<IActionResult> GetVersion()
		{
			return Ok(new ResponseDto() { IsSuccess = true, Token = "", Message = $"", Model="55" });
		}

		[HttpGet("session")]
		public async Task<IActionResult> InitSession()
		{
			var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
			var token = await _authService.InitVisitorSession(currentUserID);
			return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"" });
		}

		[HttpPost("applyemail")]
		public async Task<ActionResult<ResponseDto>> ApplyEmail([FromBody] LoginDto loginModel)
		{
			try
			{
				var token = await _authService.ApplyEmail(loginModel);
				return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"Email applied Username {loginModel.Email}" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[HttpPost("applycode")]
		public async Task<ActionResult<ResponseDto>> ApplyCode([FromBody] LoginDto loginModel)
		{
			try
			{
				var token = await _authService.ApplyCode(loginModel);
				return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[HttpPost("forgotpassword")]
		public async Task<ActionResult<ResponseDto>> ForgotPassword([FromBody] LoginDto loginModel)
		{
			try
			{
				var token = await _authService.ForgotPassword(loginModel);
				return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[HttpPost("applypassword")]
		public async Task<ActionResult<ResponseDto>> ApplyPassword([FromBody] LoginDto loginModel)
		{
			var claims = new List<Claim>();
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				var token = await _authService.Login(loginModel, currentUserID);

				return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[HttpPost("resetpassword")]
		public async Task<ActionResult<ResponseDto>> ResetPassword([FromBody] LoginDto loginModel)
		{
			var claims = new List<Claim>();
			try
			{
				await _authService.ResetPassword(loginModel);
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				var token = await _authService.Login(loginModel, currentUserID);

				return Ok(new ResponseDto() { IsSuccess = true, Token = token, Message = $"" });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

		[Authorize]
		[HttpGet("getuserrights")]
		public async Task<ActionResult<ResponseDto>> GetUserRights()
		{
			try
			{
				var currentUserID = User.FindFirstValue(JwtRegisteredClaimNames.Sid);
				var userAccessRights = await _authService.GetUserRights(currentUserID);
				return Ok(new ResponseDto { IsSuccess = true, Model = userAccessRights });
			}
			catch (Exception ex)
			{
				return Unauthorized(new ResponseDto { IsSuccess = false, Message = ex.Message });
			}
		}

	}
}
