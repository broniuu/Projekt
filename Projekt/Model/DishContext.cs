using Microsoft.EntityFrameworkCore;

namespace Projekt
{
    public class DishContext : DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<User> Users { get; set; }

        public string DbPath { get; private set; }
        
        public DishContext()
        {
            DbPath = @"C:\Users\broni\source\repos\Projekt\data.db";
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

    }
}
