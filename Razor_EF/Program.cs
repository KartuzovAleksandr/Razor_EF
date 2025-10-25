using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Razor_EF.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Razor_EF-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Используем Serilog как основной провайдер логов
builder.Host.UseSerilog(); // <-- это подключает Serilog к ILogger<T>

// Добавление DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Инициализация БД при старте
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    context.Database.EnsureCreated(); // Создаём БД, если не существует

//    if (!context.Clients.Any()) // Заполняем, если пусто
//    {
//        SeedData.Initialize(context);
//    }
//}

// https://localhost:7151/Files/
// даю каталог для скачивания
app.UseFileServer(new FileServerOptions
{
    EnableDirectoryBrowsing = true,
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Files")),
    RequestPath = new PathString("/Files"),
    EnableDefaultFiles = false
});

// меняем имя окружения
// тогда увидим переадресацию на страницу Error
// app.Environment.EnvironmentName = "Production";

// Настройка обработки ошибок на страницу Error (Razor)
// проверка https://localhost:7151/TestError
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // Для разработки — подробные ошибки
}

app.UseHttpsRedirection();

// app.UseDirectoryBrowser(); // просмотр содержимое каталогов на сайте

// app.UseDefaultFiles(); // переадресация на wwwroot

app.UseStaticFiles(); // можно открывать полные адреса из wwwroot

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// добавить стандартный обработчик ошибок ASP.net Core
// app.UseDeveloperExceptionPage();

// проверка стандартного обработчика
//app.Run(async (context) =>
//{
//    int a = 5;
//    int b = 0;
//    int c = a / b;
//    await context.Response.WriteAsync($"c = {c}");
//});

app.Run();