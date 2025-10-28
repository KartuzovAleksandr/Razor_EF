using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor_EF.Models;
using System.ComponentModel.DataAnnotations;

namespace Razor_EF;

public static class OrdersREST
{
    public static void MapOrdersApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            return await db.Orders
                .Include(o => o.Client)
                .Include(o => o.Product)
                .ToListAsync();
        });

        group.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.Client)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order is null
                ? Results.NotFound($"Order с ID {id} не найден.")
                : Results.Ok(order);
        });

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
        });

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
        });

        group.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return Results.NotFound($"Order с ID {id} не найден.");

            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
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

// DTO остаются без изменений
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