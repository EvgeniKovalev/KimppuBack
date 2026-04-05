using Kimppu.Features.Auth;
using Kimppu.Features.One;
using Microsoft.AspNetCore.Mvc;

namespace Kimppu
{
	public static class Endpoints
	{
		public static void MapCarEndpoints(this IEndpointRouteBuilder app)
		{
			var authGroup = app.MapGroup("/authentication")
										 .WithTags("Auth");
			//.RequireAuthorization(); // Secure all endpoints in this group

			// GET /api/cars (One-time load or list)
			authGroup.MapGet("/", async ([FromServices] AuthService service) =>
					Results.Ok(await service.GetVersion()));

			authGroup.MapGet("/version", async ([FromServices] AuthService service) =>
				 Results.Ok(await service.GetVersion()));




			var carGroup = app.MapGroup("/cars")
									 .WithTags("Cars");
			//.RequireAuthorization(); // Secure all endpoints in this group

			// GET /api/cars (One-time load or list)
			//carGroup.MapGet("/", async ([FromServices] CarService service) =>
			//		Results.Ok(await service.GetAllCarsAsync()));

			//carGroup.MapGet("/search", async (string? make, [FromServices] CarService service) =>
			//		Results.Ok(await service.SearchCarsAsync(make)));

			// POST /api/cars (Add a car)
			//group.MapPost("/", async (CreateCarDto dto, CarService service) => {
			//	var car = await service.CreateCarAsync(dto);
			//	return Results.Created($"/api/cars/{car.Id}", car);
			//});
		}
	}
}