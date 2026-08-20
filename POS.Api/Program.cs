using BLL;
using BLL.Models;
using DAL;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using POS.Api.Services;
using POS.Contracts.Printing;
using POS.Api.Auth;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
builder.Services.AddDalServices(connectionString);
builder.Services.AddBLL();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings missing in appsettings.json");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.Configure<RefreshTokenSettings>(builder.Configuration.GetSection("RefreshToken"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IReceiptPrinter, NullReceiptPrinter>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();
