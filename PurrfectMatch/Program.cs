var builder = WebApplication.CreateBuilder(args);

// Dodaj us³ugi MVC lub Web API
builder.Services.AddControllersWithViews();  // Dla aplikacji MVC
// builder.Services.AddControllers();        // Dla aplikacji Web API

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
