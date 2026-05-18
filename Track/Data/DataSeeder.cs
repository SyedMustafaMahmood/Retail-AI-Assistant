using Track.Models;

namespace Track.Data
{
    public class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Prevent duplicate seeding
            if (db.Products.Any())
                return;

            var products = new List<Product>
            {
                new()
                {
                    Name = "Keyboard",
                    Description = "RGB mechanical gaming keyboard"
                },

                new()
                {
                    Name = "Gaming Mouse",
                    Description = "Wireless RGB gaming mouse"
                },

                new()
                {
                    Name = "Gaming Headset",
                    Description = "Wireless RGB gaming headset"
                },

                new()
                {
                    Name = "Mouse Pad",
                    Description = "Large smooth gaming mouse pad"
                },

                new()
                {
                    Name = "USB Hub",
                    Description = "USB hub for connecting accessories"
                },

                new()
                {
                    Name = "Gaming Chair",
                    Description = "Ergonomic RGB gaming chair"
                },

                new()
                {
                    Name = "Desk Lamp",
                    Description = "RGB gaming desk lamp"
                },

                new()
                {
                    Name = "Laptop Stand",
                    Description = "Aluminium adjustable laptop stand"
                }
            };

            db.Products.AddRange(products);

            var transactions = new List<Transaction>
            {
                new()
                {
                    CustomerName = "Stephen",
                    Items = new List<TransactionItem>
                    {
                        new() { ProductName = "Keyboard" },
                        new() { ProductName = "Mouse Pad" },
                        new() { ProductName = "Gaming Mouse" }
                    }
                },

                new()
                {
                    CustomerName = "Sindu",
                    Items = new List<TransactionItem>
                    {
                        new() { ProductName = "Keyboard" },
                        new() { ProductName = "Mouse Pad" }
                    }
                },

                new()
                {
                    CustomerName = "Syed",
                    Items = new List<TransactionItem>
                    {
                        new() { ProductName = "Gaming Mouse" },
                        new() { ProductName = "Gaming Headset" }
                    }
                }
            };

            db.Transactions.AddRange(transactions);

            await db.SaveChangesAsync();
        }
    }
}