using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// All about identity
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services
    .AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<AuthDbContext>(o => 
    o.UseSqlite($"DataSource={Path.GetTempFileName()}"),
    ServiceLifetime.Singleton);

builder.Services
    .AddIdentityCore<LocalUser>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddApiEndpoints();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";
    options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .MapGet("/", (ClaimsPrincipal claim) => Results.Ok($"hello {claim.Identity.Name}"))
    .RequireAuthorization()
    .WithName("MyExampleApi")
    .WithOpenApi();

app.MapIdentityApi<LocalUser>();


app.Run();

public class LocalUser : IdentityUser
{
}

public class AuthDbContext : IdentityDbContext<LocalUser>
{
    public AuthDbContext()
    {
        Database.Migrate();
    }

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        Database.Migrate();
    }
}