using AspNetCoreRateLimit;
using Marketplace.Repository;
using Marketplace;
using Marketplace.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSetting");
var azureDeploymentSettings = builder.Configuration.GetSection("AzureSettings");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<RepositorySettings>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddSingleton<CommunicationService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AttributeService>();


builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(opt =>
{
	opt.GeneralRules = new List<RateLimitRule>() { new RateLimitRule { Endpoint = "*", Limit = 100, Period = "1m" } };
});

builder.Services.AddAuthentication(config =>
{
	config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	config.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
	config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(config =>
{
	config.SaveToken = true;
	config.RequireHttpsMetadata = false;
	config.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidAudience = builder.Environment.IsDevelopment() ? azureDeploymentSettings["FrontendAddress"] : Environment.GetEnvironmentVariable("FrontendAddress"),
		ValidIssuer = "Marketplace infrastructure",
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("SecurityKey").Value!))
	};
});


builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = @"JWT Authorization example : Bearer something123",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"},
				Scheme = "oauth2",
				Name = "Bearer",
				In = ParameterLocation.Header
			},
			new List<string>()
		}
	});
});

builder.Services.AddCors(options =>
{
	var corsAddress = builder.Environment.IsDevelopment() ? azureDeploymentSettings["FrontendAddress"] : Environment.GetEnvironmentVariable("FrontendAddress");
	options.AddPolicy(name: "MarketplaceCorsPolicy", builder =>
	{
		builder.WithOrigins(corsAddress).AllowAnyHeader().WithMethods("GET", "POST").AllowCredentials();
	});
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IISServerOptions>(opt => { opt.MaxRequestBodySize = 10 * 1024 * 1024; });
builder.Services.Configure<KestrelServerOptions>(opt => { opt.Limits.MaxRequestBodySize = 10 * 1024 * 1024; });


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MarketplaceCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();