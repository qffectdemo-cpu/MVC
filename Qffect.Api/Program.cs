using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qffect.Api.Infrastructure.Tenancy; // this is important
using Qffect.Api.Middleware;
using Qffect.Application.Employees.Handlers;
using Qffect.Application.Interfaces;
using Qffect.Domain.ADM;
using Qffect.Infrastructure.Persistence;
using Qffect.Infrastructure.Repositories;
using Qffect.SharedKernel.Tenancy;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB (shared with Web)
builder.Services.AddDbContext<QffectIdentityDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<QffectSettingsDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<QffectDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    



builder.Services.AddIdentityCore<ApplicationUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<QffectIdentityDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();
// register MediatR handlers from the Application assembly
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(EmployeeHandler).Assembly);
});
// register repository mapping: Application interface -> Infrastructure implementation
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();


// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // true in prod
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Qffect API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS for mobile
builder.Services.AddCors(p => p.AddPolicy("Mobile", b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

//For Tenancy
// Tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantResolver, HostHeaderTenantResolver>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Qffect API V1");
        c.RoutePrefix = "swagger";
    });
}


app.UseHttpsRedirection();
app.UseCors("Mobile");

// Tenancy should run early, before auth/authorization and before controllers
app.UseMiddleware<TenancyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


// Health & ping
app.MapGet("/api/v1/ping", () => Results.Ok(new { status = "ok", service = "Qffect.Api", version = "v1" }));

app.Run();

static class JwtTokenFactory
{
    public static string Create(ApplicationUser user, IConfiguration cfg)
    {
        var jwt = cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", user.Id.ToString()),
            new("name", user.UserName ?? ""),
            new("tenantId", user.TenantId.ToString())
            // TODO: add "perm" claims based on role membership
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: creds);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
