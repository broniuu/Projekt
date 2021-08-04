using System;
using System.Collections.Generic;
using consoleasync;
using Microsoft.EntityFrameworkCore;

namespace Projekt
{
    public class DishContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<DishData> DishDatas { get; set; }

        public string DbPath { get; private set; }
        
        public DishContext()
        {
            DbPath = @"C:\Users\broni\source\repos\Projekt\data.db";
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

    }
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }

        public List<DishData> DishDatas { get; } = new List<DishData>();
    }
    public class DishData
    {
        public int DishDataId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Status Availability { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
    public enum Status
    {
        unavalible,
        temporarilyUnavailable,
        avalible,
        avalibleAtSelectedTimes
    }

}
