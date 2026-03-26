namespace Marketplace.Models.DTO
{
	public class ResponseDto
	{
		public bool IsSuccess { get; set; }
		public string? Message { get; set; }
		public string? Token { get; set; }
		public object? Model { get; set; }
	}
}
