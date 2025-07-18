
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
        // Parse and rebuild the connection string to ensure it's valid
        try
        {
            var uriBuilder = new UriBuilder(connectionString);
            
            // Extract components
            var host = uriBuilder.Host;
            var port = uriBuilder.Port == -1 ? 5432 : uriBuilder.Port; // Default PostgreSQL port
            var database = uriBuilder.Path.TrimStart('/');
            var username = uriBuilder.UserName;
            var password = uriBuilder.Password;
            
            // Build proper Npgsql connection string
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            // Fallback to local configuration if parsing fails
            Console.WriteLine($"Failed to parse DATABASE_URL: {ex.Message}");
            connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Database=ModernBlog;Username=postgres;Password=postgres";
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

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
    
    // Forçar criação do banco e tabelas
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Banco de dados e tabelas criados/verificados!");
    
    // Uncomment the line below to reset database (clear all data)
    // await SeedData.ResetDatabaseAsync(services);
    
    await SeedData.InitializeAsync(services);
    
    Console.WriteLine("✅ Dados de seed carregados!");
}

app.Run("http://0.0.0.0:5000");
