using Kimppu.Shared.Models;

namespace Kimppu.Features.Auth
{
	public class AuthService
	{
		public async Task<CommonResponse> GetVersion()
		{
			await Task.Delay(1000);
			var random = new Random();
			return new CommonResponse() { IsSuccess = true, Token = "", Message = $"", Model = random.Next().ToString() };
		}
	}
}
