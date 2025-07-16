
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Data;
using ModernBlog.Models;
using ModernBlog.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use PostgreSQL for development/Replit, easily changeable to SQL Server
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Fix malformed connection string if it ends with incomplete sslmode parameter
        if (connectionString.EndsWith("?sslmode"))
        {
            connectionString += "=require";
        }
        else if (connectionString.EndsWith("?sslmode="))
        {
            connectionString += "require";
        }
        // Add SSL mode if not present
        else if (!connectionString.Contains("sslmode="))
        {
            connectionString += connectionString.Contains("?") ? "&sslmode=require" : "?sslmode=require";
        }
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=ModernBlog;Username=postgres;Password=postgres";
    }
        
    options.UseNpgsql(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add custom services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Create admin user and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

app.Run("http://0.0.0.0:5000");
