using Track.Models;
using Transaction = Track.Models.Transaction;

namespace Track.Data
{
    public class DataSeeder
    {
        private readonly AppDbContext _db;

        public DataSeeder(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            if (_db.Products.Any())
                return;

            var products = new List<Product>
            {
                // Phones
                new() { Name = "Redmi Note 12", Description = "Budget smartphone with excellent battery life" },
                new() { Name = "iPhone 13", Description = "Premium smartphone with great camera and performance" },
                new() { Name = "Samsung Galaxy M34", Description = "Mid-range phone with strong battery and display" },

                // Phone Accessories
                new() { Name = "Phone Case", Description = "Protective case compatible with most smartphones" },
                new() { Name = "Screen Protector", Description = "Tempered glass screen protector for smartphones" },
                new() { Name = "Wireless Charger", Description = "Fast wireless charger compatible with all Qi devices" },
                new() { Name = "Earphones", Description = "Wired earphones with mic and noise isolation" },
                new() { Name = "Power Bank", Description = "20000mAh portable power bank with fast charging" },

                // Gaming Peripherals
                new() { Name = "Keyboard", Description = "RGB mechanical gaming keyboard" },
                new() { Name = "Gaming Mouse", Description = "Wireless RGB gaming mouse" },
                new() { Name = "Gaming Headset", Description = "Wireless RGB gaming headset" },
                new() { Name = "Mouse Pad", Description = "Large smooth gaming mouse pad" },
                new() { Name = "USB Hub", Description = "USB hub for connecting multiple accessories" },
                new() { Name = "Gaming Chair", Description = "Ergonomic RGB gaming chair" },
                new() { Name = "Desk Lamp", Description = "RGB gaming desk lamp" },
                new() { Name = "Laptop Stand", Description = "Aluminium adjustable laptop stand" }
            };

            _db.Products.AddRange(products);

            var transactions = new List<Transaction>
            {
                // Phone buyers
                new()
                {
                    CustomerName = "Stephen",
                    Items = new()
                    {
                        new() { ProductName = "iPhone 13" },
                        new() { ProductName = "Phone Case" },
                        new() { ProductName = "Screen Protector" },
                        new() { ProductName = "Wireless Charger" }
                    }
                },
                new()
                {
                    CustomerName = "Sindu",
                    Items = new()
                    {
                        new() { ProductName = "Samsung Galaxy M34" },
                        new() { ProductName = "Phone Case" },
                        new() { ProductName = "Screen Protector" }
                    }
                },
                new()
                {
                    CustomerName = "Syed",
                    Items = new()
                    {
                        new() { ProductName = "Redmi Note 12" },
                        new() { ProductName = "Phone Case" },
                        new() { ProductName = "Power Bank" },
                        new() { ProductName = "Earphones" }
                    }
                },
                new()
                {
                    CustomerName = "Raj",
                    Items = new()
                    {
                        new() { ProductName = "iPhone 13" },
                        new() { ProductName = "Wireless Charger" },
                        new() { ProductName = "Earphones" }
                    }
                },
                new()
                {
                    CustomerName = "Priya",
                    Items = new()
                    {
                        new() { ProductName = "Samsung Galaxy M34" },
                        new() { ProductName = "Power Bank" },
                        new() { ProductName = "Screen Protector" }
                    }
                },

                // Gaming buyers
                new()
                {
                    CustomerName = "Alex",
                    Items = new()
                    {
                        new() { ProductName = "Keyboard" },
                        new() { ProductName = "Gaming Mouse" },
                        new() { ProductName = "Mouse Pad" },
                        new() { ProductName = "Gaming Headset" }
                    }
                },
                new()
                {
                    CustomerName = "Maya",
                    Items = new()
                    {
                        new() { ProductName = "Keyboard" },
                        new() { ProductName = "Mouse Pad" },
                        new() { ProductName = "USB Hub" }
                    }
                },
                new()
                {
                    CustomerName = "Arjun",
                    Items = new()
                    {
                        new() { ProductName = "Gaming Mouse" },
                        new() { ProductName = "Gaming Headset" },
                        new() { ProductName = "Gaming Chair" }
                    }
                },
                new()
                {
                    CustomerName = "Neha",
                    Items = new()
                    {
                        new() { ProductName = "Laptop Stand" },
                        new() { ProductName = "USB Hub" },
                        new() { ProductName = "Desk Lamp" },
                        new() { ProductName = "Keyboard" }
                    }
                },
                new()
                {
                    CustomerName = "Kiran",
                    Items = new()
                    {
                        new() { ProductName = "Gaming Chair" },
                        new() { ProductName = "Desk Lamp" },
                        new() { ProductName = "Mouse Pad" }
                    }
                }
            };

            _db.Transactions.AddRange(transactions);
            await _db.SaveChangesAsync();
        }
    }
}