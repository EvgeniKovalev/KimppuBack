namespace Kimppu.Shared.Models
{
	public class CommonResponse
	{
		public string Message { get; set; }
		public object Model { get; set; }
		public string Token { get; set; }
		public bool IsSuccess { get; set; }
	}
}