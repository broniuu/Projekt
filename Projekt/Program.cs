using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using Projekt;

namespace consoleasync
{

    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World with C# 9.0!");
            Console.WriteLine("--------------------------------------------------");
            // Klitka U Witka
            //var dishesFromKiltkaUWitka = await DishParserFromKlitkaUWitka.FindDishes();
            //Console.WriteLine(string.Join(Environment.NewLine, dishesFromKiltkaUWitka.Select(d => $"{d.Name} - {d.Price} - {d.Availability}")));

            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();

            // Imperial Restauracja
            var dishesFromImperialrestauracja = await DishParserFromImperialrestauracja.FindDishes();
            Console.WriteLine(string.Join(Environment.NewLine, dishesFromImperialrestauracja.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));

            var restaurantName = "Imperial Restauracja";
            await UpsertDishes(dishesFromImperialrestauracja, restaurantName);

            // Klitka u witka
            Console.WriteLine("-------------------------------------------");
            var dishesFromKlitkaUWitka = await DishParserFromKlitkaUWitka.FindDishes() ;

            Console.WriteLine(string.Join(Environment.NewLine, dishesFromKlitkaUWitka.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));
            
            restaurantName = "Klitka U Witka";
            await UpsertDishes(dishesFromKlitkaUWitka, restaurantName);
        }

        //Funkcja UPSERT
        private static Task UpsertDishes(IEnumerable<MinimalDish> minimalDishesFromRestaurant, string restaurantName)
        {
            using (var db = new DishContext())
            {
                // Read
                Console.WriteLine("Querying for a Restaurant");
                var restaurant = db.Restaurants
                    .FirstOrDefault(r => r.Name.Equals(restaurantName));
                var dish1 = db.Dishes;
                if (restaurant == null)
                {
                    // Create
                    Console.WriteLine($"Inserting {restaurantName}");
                    restaurant = db.Add(entity: new Restaurant { Name = restaurantName }).Entity;
                    db.SaveChanges();
                }

                Console.WriteLine($"Upserting dishes form {restaurant}");
                //var imperialDishesData = restaurant.DishDatas;
                foreach (var minimalDish in minimalDishesFromRestaurant)
                {
                    //Read
                    var dishes = db.Dishes
                        .FirstOrDefault(d => d.Name.Equals(minimalDish.Name) && d.RestaurantId == restaurant.RestaurantId);
                    if (dishes == null)
                    {
                        //Insert
                        restaurant.Dishes.Add(new Dish
                        {
                            Name = minimalDish.Name,
                            Price = minimalDish.Price,
                            Availability = minimalDish.Availability
                        });
                    }
                    else
                    {
                        //Update
                        dishes.Price = minimalDish.Price;
                        dishes.Availability = minimalDish.Availability;
                    }
                }
                db.SaveChanges();
                //Delete
                var dishesFromRestaurant = db.Dishes.Where(d => d.RestaurantId == restaurant.RestaurantId);
                foreach (var dish in dishesFromRestaurant)
                {
                    var minimalDish = minimalDishesFromRestaurant.FirstOrDefault(m => m.Name.Equals(dish.Name));
                    if (minimalDish == null)
                    {
                        db.Remove(dish);
                    }
                }
                db.SaveChanges();

            }
            return Task.CompletedTask;
        }
    }
}
