using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Configuration;
using Razor_EF.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// REST API /api/orders уже использует настроенный JWT Bearer
// (из Program.cs с cookie), поэтому защита можно сделать и так:
// добавляем [Authorize] на группу или отдельные endpoints

// Политики из Program.cs
// GET — Manager/Admin
// DELETE — Admin
// POST/PUT — User/Manager/Admin

namespace Razor_EF;

public static class OrdersREST
{
    public static void MapOrdersApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            return await db.Orders.Include(o => o.Client).
                                   Include(o => o.Product).
                                   ToListAsync();
        }).RequireAuthorization("ManagerAdmin");
        // добавил RequireAuthorization("ManagerAdmin"); 

        group.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var order = await db.Orders.Include(o => o.Client).
                                        Include(o => o.Product).
                                        FirstOrDefaultAsync(o => o.Id == id);
            return order is null ? Results.NotFound($"Заказ {id} не найден.") : Results.Ok(order);
        }).RequireAuthorization("ManagerAdmin");

        group.MapPost("/", async (CreateOrderDto dto, ApplicationDbContext db) =>
        {
            var errors = Validate(dto);
            if (errors.Count > 0)
                return Results.BadRequest(new ValidationProblemDetails(errors));

            var clientExists = await db.Clients.AnyAsync(c => c.Id == dto.ClientId);
            var productExists = await db.Products.AnyAsync(p => p.Id == dto.ProductId);

            if (!clientExists)
                return Results.BadRequest("Клиент с указанным ID не существует.");
            if (!productExists)
                return Results.BadRequest("Товар с указанным ID не существует.");

            var order = new Order
            {
                Date = dto.Date,
                ClientId = dto.ClientId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return Results.Created($"/api/orders/{order.Id}", order);
        }).RequireAuthorization("UserAny"); 
        // добавил для любого пользователя 

        group.MapPut("/{id}", async (int id, UpdateOrderDto dto, ApplicationDbContext db) =>
        {
            var errors = Validate(dto);
            if (errors.Count > 0)
                return Results.BadRequest(new ValidationProblemDetails(errors));

            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return Results.NotFound($"Order с ID {id} не найден.");

            var clientExists = await db.Clients.AnyAsync(c => c.Id == dto.ClientId);
            var productExists = await db.Products.AnyAsync(p => p.Id == dto.ProductId);

            if (!clientExists)
                return Results.BadRequest("Клиент с указанным ID не существует.");
            if (!productExists)
                return Results.BadRequest("Товар с указанным ID не существует.");

            order.Date = dto.Date;
            order.ClientId = dto.ClientId;
            order.ProductId = dto.ProductId;
            order.Quantity = dto.Quantity;

            await db.SaveChangesAsync();

            return Results.Ok(order);
        }).RequireAuthorization("UserAny");

        group.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return Results.NotFound($"Order с ID {id} не найден.");

            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization("AdminOnly");
        // только админам

        // 🔥 Новый endpoint для API клиентов
        group.MapPost("/jwt", async (LoginApiDto login, ApplicationDbContext db, IConfiguration config, ILogger<Program> logger) =>
        {
            // Поиск пользователя
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == login.UserName);
            if (user == null)
                return Results.BadRequest(new { error = "Пользователь не найден" });

            // Проверка пароля (BCrypt как в LoginModel)
            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return Results.BadRequest(new { error = "Неверный пароль" });

            // Генерация JWT (точно как в LoginModel)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("UserId", user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            logger.LogInformation($"API JWT выдан для {user.UserName} ({user.Role})");

            return Results.Ok(new
            {
                token = tokenString,
                role = user.Role.ToString(),
                expires = DateTime.UtcNow.AddHours(1)
            });
        }).AllowAnonymous();  // Доступ без авторизации
    }

    // Хелпер валидации, интегрируемый в любой endpoint
    private static Dictionary<string, string[]> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, results, true);

        var errors = new Dictionary<string, string[]>();
        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                if (!errors.ContainsKey(memberName))
                    errors[memberName] = new string[] { result.ErrorMessage ?? "" };
                else
                    errors[memberName] = errors[memberName].Append(result.ErrorMessage ?? "").ToArray();
            }
        }
        return errors;
    }
}

// DTO
public class CreateOrderDto
{
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Клиент обязателен")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Товар обязателен")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Количество обязательно")]
    [Range(1, 1000, ErrorMessage = "Количество от 1 до 1000")]
    public int Quantity { get; set; }
}

public class UpdateOrderDto
{
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Клиент обязателен")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Товар обязателен")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Количество обязательно")]
    [Range(1, 1000, ErrorMessage = "Количество от 1 до 1000")]
    public int Quantity { get; set; }
}

// Класс с именем-паролем для получения токена
public class LoginApiDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}