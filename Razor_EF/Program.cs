using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Razor_EF.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавление DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Инициализация БД при старте
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated(); // Создаём БД, если не существует

    if (!context.Clients.Any()) // Заполняем, если пусто
    {
        SeedData.Initialize(context);
    }
}

// https://localhost:7151/Files/
// даю каталог для скачивания
app.UseFileServer(new FileServerOptions
{
    EnableDirectoryBrowsing = true,
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Files")),
    RequestPath = new PathString("/Files"),
    EnableDefaultFiles = false
});

app.UseHttpsRedirection();

// app.UseDirectoryBrowser(); // просмотр содержимое каталогов на сайте

// app.UseDefaultFiles(); // переадресация на wwwroot

app.UseStaticFiles(); // можно открывать полные адреса из wwwroot

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();