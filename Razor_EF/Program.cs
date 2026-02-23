using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Razor_EF;
using Razor_EF.Models;
using Serilog;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// добавление Web API
builder.Services.AddEndpointsApiExplorer();

#region Swagger
// Swagger интерфейс https://localhost:7151/swagger

// без авторизации работает без проблем
// если уберете везде .RequireAuthorization("ManagerAdmin");
// из OrderREST.cs
// и будете использовать для тестирования Orders.http
// builder.Services.AddSwaggerGen();

// Как взять токен из браузера
// /Login → логин (например, Admin).
// F12 → Application/Storage → Cookies → http://localhost:7151 → AccessToken (JWT)
// Копируем значение полностью (начинается с eyJ...)
// запускаем /swagger.
// Кнопка Authorize → введите Bearer [токен из cookie].
// тестируем endpoints — роли проверятся

// для тестирования endpointds руками используем OprdersJwt.hhtp
// для получения токенов есть специальный адрес /api/orders/jwt
// там вернется json файл формата 
// {
//  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
//  "role": "Admin",
//  "expires": "2026-02-23T20:17:00Z"
// }
// этот токен потом можно использовать для внешних (React, мобильных) приложений 

// Swagger — с авторизацией через токен
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Тестирование(Postman / Swagger)
// User: POST / PUT OK, GET/DELETE 403.
// Manager: GET / POST / PUT OK, DELETE 403.
// Admin: Всё OK.
// Без токена: 401 на все.
// Токен берется из /Login (cookie AccessToken) или из /api/orders/jwt
// для доступа с других интерфейсов (React, PHP, Django, Mobile)
// API вернёт JSON-ошибки (не редирект, как Razor Pages)

#endregion

#region Serilog
// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Razor_EF-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Используем Serilog как основной провайдер логов
builder.Host.UseSerilog(); // <-- это подключает Serilog к ILogger<T>
#endregion

#region SwitchDB
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
#endregion

// Add services to the container.
builder.Services.AddRazorPages();

#region JwtToken
// --- НАСТРОЙКА JWT ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Jwt:Key не настроен в appsettings.json");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Для разработки true в продакшене
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Для упрощения примера
        ValidateAudience = false,
        // Используем стандартные типы Claim, которые заданы в LoginModel через ClaimTypes
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    // 1. Читаем токен из Cookie, чтобы работали обычные формы Razor Pages
    options.Events = new JwtBearerEvents
    {
        // Всегда вызывайте context.HandleResponse() и
        // return Task.CompletedTask в конце,
        // иначе возникнет ошибка "response has already started".
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        // 2. Если доступа нет, перенаправляем на Login, а не возвращаем 401
        OnChallenge = context =>
        {
            context.HandleResponse();
            // лог 
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning($"Доступ запрещён (401). Вы не вошли в систему !");
            context.Response.Redirect("/Login");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            // Нет нужды в HandleResponse() — OnForbidden вызывается
            // до стандартного 403, и редирект прерывает пайплайн
            // лог 
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning($"Доступ запрещён (403) для пользователя " +
                $"{context.Principal?.Identity?.Name}. " +
                $"Роль не подходит.");
            // 403: роль не подходит
            context.Response.Redirect("/Login?error=role");
            // Task.CompletedTask завершает обработчик корректно
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // политики для API и Swagger,
    // для страниц Razor и методов в них достаточно атрибута [Authorize]
    options.AddPolicy("ManagerAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("UserAny", policy => policy.RequireRole("User", "Manager", "Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
#endregion

var app = builder.Build();

// включение Swagger в Development
// https://localhost:7151/swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
// нужно также для стилей Bootstrap и JQuery
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