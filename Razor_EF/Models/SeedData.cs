using Bogus;

namespace Razor_EF.Models
{
    public class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Clients
            var clients = new Faker<Client>("ru")
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .Generate(5);

            context.Clients.AddRange(clients);

            // Products
            var products = new Faker<Product>("ru")
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
                .Generate(10);

            context.Products.AddRange(products);

            context.SaveChanges();

            // Orders
            var orders = new Faker<Order>("ru")
                .RuleFor(o => o.Date, f => f.Date.Past(1))
                .RuleFor(o => o.ClientId, f => f.Random.Int(1, clients.Count))
                .RuleFor(o => o.ProductId, f => f.Random.Int(1, products.Count))
                .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
                .Generate(10);

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
