using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja po��czenia z baz� danych dla kot�w
builder.Services.AddDbContext<CatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatDbContext")));

// Konfiguracja po��czenia z baz� danych dla u�ytkownik�w
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext")));

// Dodanie us�ug to�samo�ci i konfiguracja u�ytkownik�w
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Dodanie us�ug MVC
builder.Services.AddControllersWithViews();

// Konfiguracja ciasteczek aplikacji, w tym �cie�ki logowania i �cie�ki dost�pu
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // �cie�ka logowania
    options.AccessDeniedPath = "/Account/AccessDenied"; // �cie�ka w przypadku braku dost�pu
    options.Cookie.Name = "PurrfectMatchCookie"; // Nazwa ciasteczka
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var roleExist = await roleManager.RoleExistsAsync("Administrator");
    if (!roleExist)
    {
        var role = new IdentityRole("Administrator");
        await roleManager.CreateAsync(role); // Tworzymy rol�
    }

    // Dodajemy administratora, je�li go nie ma
    var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = "admin@admin.com", Email = "admin@admin.com" };
        var createAdminResult = await userManager.CreateAsync(adminUser, "AdminPassword123!");
        if (createAdminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator"); // Przypisujemy rol� administratora
        }
    }
    }


    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// U�ywanie systemu autentykacji i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 
app.Run();
