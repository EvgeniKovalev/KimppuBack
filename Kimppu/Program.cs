using Kimppu;
using Kimppu.Features.Auth;
using Kimppu.Features.One;

var builder = WebApplication.CreateBuilder(args);
var azureDeploymentSettings = builder.Configuration.GetSection("AzureSettings");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(); // Or Cookie auth

builder.Services.AddSingleton<CarService>();
builder.Services.AddSingleton<AuthService>();


builder.Services.AddCors(options =>
{
	var corsAddress = builder.Environment.IsDevelopment() ? azureDeploymentSettings["FrontendAddress"] : Environment.GetEnvironmentVariable("FrontendAddress");
	options.AddPolicy(name: "CorsPolicy", builder =>
	{
		builder.WithOrigins(corsAddress).AllowAnyHeader().WithMethods("GET", "POST").AllowCredentials();
	});
});

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapCarEndpoints();

app.Run();