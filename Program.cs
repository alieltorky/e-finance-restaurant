using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Middleware;
using Online_Restaurant.Extensions;
using Online_Restaurant.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppdbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        //  automatic retries 3lshan el database connection fails
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        );
    }));

//options in the signin
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppdbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Or /Account/Login
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
builder.Services.AddHttpContextAccessor();
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("DEVELOPMENT");
    app.UseDeveloperExceptionPage();
}
else
{
    Console.WriteLine("PRODUCTION");
    app.UseExceptionHandler();
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomRequestLogging();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// add the admin email
// Seed Default Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@restaurant.com";
    var adminUserName = "Ahmed Khalifa";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true,
            Address = "Headquarters"
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

// Seed Delivery Roles and Users
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Delivery"))
    {
        await roleManager.CreateAsync(new IdentityRole("Delivery"));
    }

    var delivery1 = await userManager.FindByNameAsync("delivery1@restaurant.com");
    if (delivery1 == null)
    {
        var user1 = new ApplicationUser
        {
            UserName = "DeliveryMan1",
            Email = "delivery1@restaurant.com",
            EmailConfirmed = true,
            PhoneNumber = "01011111111",
            Address = "Giza, Egypt"
        };
        var result = await userManager.CreateAsync(user1, "Delivery@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user1, "Delivery");
        }
    }

    var delivery2 = await userManager.FindByNameAsync("delivery2@restaurant.com");
    if (delivery2 == null)
    {
        var user2 = new ApplicationUser
        {
            UserName = "DeliveryMan2",
            Email = "delivery2@restaurant.com",
            EmailConfirmed = true,
            PhoneNumber = "01022222222",
            Address = "Cairo, Egypt"
        };
        var result = await userManager.CreateAsync(user2, "Delivery@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user2, "Delivery");
        }
    }
}

// Seed Chef Role and a default Chef user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Chef"))
    {
        await roleManager.CreateAsync(new IdentityRole("Chef"));
    }

    var chef1 = await userManager.FindByNameAsync("chef1@restaurant.com");
    if (chef1 == null)
    {
        var chefUser = new ApplicationUser
        {
            UserName = "Chef1",
            Email = "chef1@restaurant.com",
            EmailConfirmed = true,
            PhoneNumber = "01033333333",
            Address = "Restaurant Kitchen"
        };
        var result = await userManager.CreateAsync(chefUser, "Chef@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(chefUser, "Chef");
        }
    }
}

app.Run();