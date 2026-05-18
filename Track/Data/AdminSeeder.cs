using Track.Models;
using Track.Data;

namespace Track.Data
{
    public static class AdminSeeder
    {
        public static void SeedAdmin(AppDbContext db)
        {
            if (!db.Users.Any(x => x.Role == "Admin"))
            {
                var admin = new User
                {
                    Name = "Admin",

                    Email = "admin@gmail.com",

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            "Admin@123"),

                    Role = "Admin"
                };

                db.Users.Add(admin);

                db.SaveChanges();
            }
        }
    }
}