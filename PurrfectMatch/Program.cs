using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja po³¹czenia z baz¹ danych dla kotów
builder.Services.AddDbContext<CatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatDbContext")));

// Konfiguracja po³¹czenia z baz¹ danych dla u¿ytkowników
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext")));

// Dodanie us³ug to¿samoœci i konfiguracja u¿ytkowników
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Dodanie us³ug MVC
builder.Services.AddControllersWithViews();

// Konfiguracja ciasteczek aplikacji, w tym œcie¿ki logowania i œcie¿ki dostêpu
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Œcie¿ka logowania
    options.AccessDeniedPath = "/Account/AccessDenied"; // Œcie¿ka w przypadku braku dostêpu
    options.Cookie.Name = "PurrfectMatchCookie"; // Nazwa ciasteczka
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Sprawdzenie, czy istnieje jakikolwiek u¿ytkownik w bazie danych
    var user = await userManager.FindByEmailAsync("test@test.com");
    if (user == null)
    {
        // Jeœli u¿ytkownik nie istnieje, utwórz nowego
        user = new ApplicationUser { UserName = "test@test.com", Email = "test@test.com" };
        var result = await userManager.CreateAsync(user, "Test@123");

        if (result.Succeeded)
        {
            // Jeœli u¿ytkownik zosta³ utworzony, przypisz rolê (jeœli potrzebujesz)
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

// U¿ywanie systemu autentykacji i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

// Zmiana domyœlnej œcie¿ki na rejestracjê
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Zmieniamy domyœlny kontroler i akcjê na rejestracjê

app.Run();
