using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using noteapp.Data;
using System.Text;
using noteapp.Filters;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load .env file
Env.Load();

// ✅ Read env variables
var dbConn = Environment.GetEnvironmentVariable("DB_CONNECTION");

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// ✅ Null check (IMPORTANT)
if (string.IsNullOrEmpty(dbConn))
    throw new Exception("DB_CONNECTION is missing in .env file");

if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT_KEY is missing in .env file");

if (string.IsNullOrEmpty(jwtIssuer))
    throw new Exception("JWT_ISSUER is missing in .env file");

if (string.IsNullOrEmpty(jwtAudience))
    throw new Exception("JWT_AUDIENCE is missing in .env file");

// DB Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(dbConn, ServerVersion.AutoDetect(dbConn))
);

// JWT Setup
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger JWT Setup
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "noteapp",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {token}"
    });

    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// ✅ Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
