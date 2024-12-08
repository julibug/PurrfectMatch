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

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

//    var roleExist = await roleManager.RoleExistsAsync("Administrator");
//    if (!roleExist)
//    {
//        var role = new IdentityRole("Administrator");
//        await roleManager.CreateAsync(role); // Tworzymy rolê
//    }

//    // Dodajemy administratora, jeœli go nie ma
//    var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
//    if (adminUser == null)
//    {
//        adminUser = new ApplicationUser { UserName = "admin@admin.com", Email = "admin@admin.com" };
//        var createAdminResult = await userManager.CreateAsync(adminUser, "AdminPassword123!");
//        if (createAdminResult.Succeeded)
//        {
//            await userManager.AddToRoleAsync(adminUser, "Administrator"); // Przypisujemy rolê administratora
//        }
//    }
//}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatDbContext>();

    // Opcjonalnie: automatyczne zastosowanie migracji
    context.Database.Migrate();

    // Dodanie danych na sztywno, jeœli ich jeszcze nie ma
    if (!context.AdoptionRequests.Any())
    {
        context.AdoptionRequests.AddRange(
            new AdoptionRequest
            {
                UserId = "1",
                CatId = 1,
                HasOtherAnimals = true,
                HasChildren = false,
                Housing = true
            },
            new AdoptionRequest
            {
                UserId = "2",
                CatId = 2,
                HasOtherAnimals = false,
                HasChildren = true,
                Housing = false
            },
            new AdoptionRequest
            {
                UserId = "3",
                CatId = 3,
                HasOtherAnimals = true,
                HasChildren = true,
                Housing = true
            }
        );

        // Zapisz zmiany w bazie
        context.SaveChanges();
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
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Zmieniamy domyœlny kontroler i akcjê na rejestracjê

app.Run();
