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
            var Items = new List<string> { "iPhone 17", "IQOO 15", "Samsung S25 Ultra", 
                "Samsung A36", "Samsung S25 FE", "Redmi K90", "Realme P3", "Techo Spark 40",
                "Techo Spark Slim", "Infinix Hot 60+", "iPhone 17 Air"};
            var products = new Faker<Product>("ru")
                .RuleFor(p => p.Name, f => f.PickRandom(Items))
                .RuleFor(p => p.Price, f => f.Random.Int(10000, 150000))
                .Generate(12);

            context.Products.AddRange(products);

            context.SaveChanges();

            // Orders
            var orders = new Faker<Order>("ru")
                // последние 30 дней
                .RuleFor(o => o.Date, f => DateTime.UtcNow.AddDays(-f.Random.Number(1, 30)))
                // .RuleFor(o => o.Date, f => DateTime.Now.ToUniversalTime())
                .RuleFor(o => o.ClientId, f => f.Random.Int(1, clients.Count))
                .RuleFor(o => o.ProductId, f => f.Random.Int(1, products.Count))
                .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
                .Generate(10);

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
