using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qffect.Domain.ADM;
using Qffect.Infrastructure.Identity;
using Qffect.Infrastructure.Persistence;
using Qffect.SharedKernel.Tenancy;
using Qffect.Web.Infrastructure.Tenancy;
using Qffect.Web.Middleware;
using Yarp.ReverseProxy;


// Build
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
// --- DB (Identity + Settings + later business DbContext) ---
builder.Services.AddDbContext<QffectIdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Qffect.Infrastructure")
    ));

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<QffectIdentityDbContext>();

builder.Services.AddDbContext<QffectSettingsDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity (with 2FA support) ---
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.SignIn.RequireConfirmedAccount = true;  // enable email confirm in prod
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<QffectIdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Module authorization (policy-based)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, Qffect.Web.Infrastructure.Auth.ModulePolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, Qffect.Web.Infrastructure.Auth.ModuleAuthorizationHandler>();


// Auth cookie
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.HttpOnly = true;
    opts.LoginPath = "/Identity/Account/Login";
    opts.AccessDeniedPath = "/Identity/Account/AccessDenied";
    opts.SlidingExpiration = true;
});

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


// Authorization Policies (permissions wired later by claims)
builder.Services.AddAuthorization();

// Tenancy
builder.Services.AddScoped<ITenantResolver, HostHeaderTenantResolver>();


// Build app
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<TenancyMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

/// Enable Razor Pages (Identity uses them)
app.MapRazorPages();

// Redirect root URL ("/") to the Identity Login page
app.MapGet("/", context =>
{
    context.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});

// Area + default routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



// Seed identity (dev)
using (var scope = app.Services.CreateScope())
{
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await IdentitySeeder.SeedAsync(roles, users);
}
app.MapReverseProxy();

app.Run();
