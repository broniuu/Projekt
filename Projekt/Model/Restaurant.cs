using System.Collections.Generic;

namespace Projekt
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }

        public List<Dish> Dishes { get; } = new List<Dish>();
    }
}
