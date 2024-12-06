using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatDbContext>();

    if (!dbContext.Cats.Any()) // Jeœli tabela jest pusta
    {
        dbContext.Cats.AddRange(new List<Cat>
        {
            new Cat { Name = "Mruczek", Age = 2, Description = "Uroczy kot", IsAvailable = true },
            new Cat { Name = "Feliks", Age = 5, Description = "Kot o smuk³ej sylwetce", IsAvailable = false }
        });
        dbContext.SaveChanges(); // Zapisz zmiany do bazy danych
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cats}/{action=Index}/{id?}");

app.Run();
