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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Sprawdzenie, czy istnieje jakikolwiek u�ytkownik w bazie danych
    var user = await userManager.FindByEmailAsync("test@test.com");
    if (user == null)
    {
        // Je�li u�ytkownik nie istnieje, utw�rz nowego
        user = new ApplicationUser { UserName = "test@test.com", Email = "test@test.com" };
        var result = await userManager.CreateAsync(user, "Test@123");

        if (result.Succeeded)
        {
            // Je�li u�ytkownik zosta� utworzony, przypisz rol� (je�li potrzebujesz)
            // var roleResult = await roleManager.CreateAsync(new IdentityRole("User"));
            // if (roleResult.Succeeded)
            // {
            //     await userManager.AddToRoleAsync(user, "User");
            // }
            Console.WriteLine("Test user created successfully!");
        }
        else
        {
            Console.WriteLine("Error creating test user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// U�ywanie systemu autentykacji i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

// Zmiana domy�lnej �cie�ki na rejestracj�
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Zmieniamy domy�lny kontroler i akcj� na rejestracj�

app.Run();
