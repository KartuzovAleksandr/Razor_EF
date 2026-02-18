using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Razor_EF;
using Razor_EF.Models;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Swagger интерфейс https://localhost:7151/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Razor_EF-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Используем Serilog как основной провайдер логов
builder.Host.UseSerilog(); // <-- это подключает Serilog к ILogger<T>

// получаем строку подключения из файла конфигурации
// string? connection = builder.Configuration.GetConnectionString("MsDocker");
string? con1 = builder.Configuration.GetConnectionString("SqlExpress");
string? con2 = builder.Configuration.GetConnectionString("SQLite");
string? con3 = builder.Configuration.GetConnectionString("Postgres");

string? Db = "SQLite";

// добавляем контекст ApplicationContext в качестве сервиса в приложение
switch (Db)
{
    case "Postgres":
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(con3));
        break;
    case "SQLite":
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(con2));
        break;
    case "SqlExpress":
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(con1));
        break;
    default:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(con2));
        break;
}

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //// указывает, будет ли валидироваться издатель при валидации токена
            //ValidateIssuer = true,
            //// строка, представляющая издателя
            //ValidIssuer = AuthOptions.ISSUER,
            //// будет ли валидироваться потребитель токена
            //ValidateAudience = true,
            //// установка потребителя токена
            //ValidAudience = AuthOptions.AUDIENCE,
            //// будет ли валидироваться время существования
            //ValidateLifetime = true,
            //// установка ключа безопасности
            //IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            //// валидация ключа безопасности
            //ValidateIssuerSigningKey = true,
        };
    });

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// включение Swagger в Development
// https://localhost:7151/swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Инициализация БД при старте
// убрал в Razor Pages для правильной обработки ошибки (отдельно для Development и Production)
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
// даем каталог для скачивания / просмотра
// не будет работать в Docker - потому что доступ к физическому каталогу
//app.UseFileServer(new FileServerOptions
//{
//    EnableDirectoryBrowsing = true,
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Files")),
//    RequestPath = new PathString("/Files"),
//    EnableDefaultFiles = false
//});

// меняем имя окружения
// тогда увидим переадресацию на страницу Error
// app.Environment.EnvironmentName = "Production";

// Настройка обработки ошибок на страницу Error (Razor)
// проверка https://localhost:7151/TestError
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // для обработки ошибок HTTP
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
    app.UseHsts();

    // реальная защита от DDOS - не заработала
    //builder.Services.AddRateLimiter(options =>
    //{
    //    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    //        RateLimitPartition.GetFixedWindowLimiter(
    //            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
    //            factory: partition => new FixedWindowRateLimiterOptions
    //            {
    //                AutoReplenishment = true,
    //                PermitLimit = 10,  // 10 запросов за окно
    //                Window = TimeSpan.FromMinutes(1)
    //            }));
    //});

    //// ДО MapRazorPages() - защита от DDOS
    //app.UseRateLimiter();
}
else
{
    app.UseDeveloperExceptionPage(); // Для разработки — подробные ошибки
}

app.UseHttpsRedirection();

// просмотр содержимое каталогов на сайте - которые укажем сами выше
// в app.UseFileServer
// app.UseDirectoryBrowser(); 

// переадресация на wwwroot
// не использовать вместе с UseDirectoryBrower работать не будет
// app.UseDefaultFiles(); 

// можно открывать файлы из wwwroot без указания полного пути
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// добавить стандартный обработчик ошибок ASP.net Core
//app.UseDeveloperExceptionPage();

// проверка стандартного обработчика
//app.Run(async (context) =>
//{
//    int a = 5;
//    int b = 0;
//    int c = a / b;
//    await context.Response.WriteAsync($"c = {c}");
//});

// Подключаем REST API для Orders
app.MapOrdersApi();

app.Run();