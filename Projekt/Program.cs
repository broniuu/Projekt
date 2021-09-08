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

            //var restaurantName = "Imperial Restauracja";
            //UpsertDishes(dishesFromImperialrestauracja, restaurantName);

            // Klitka u witka
            Console.WriteLine("-------------------------------------------");
            var dishesFromKlitkaUWitka = await DishParserFromKlitkaUWitka.FindDishes() ;

            Console.WriteLine(string.Join(Environment.NewLine, dishesFromKlitkaUWitka.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));
        }

        //Funkcja UPSERT
        private static void UpsertDishes(List<Dish> dishesFromRestaurant, string restaurantName)
        {
            using (var db = new DishContext())
            {
                // Read
                Console.WriteLine("Querying for a Restaurant");
                var restaurant = db.Restaurants
                    .FirstOrDefault(r => r.Name.Equals(restaurantName));
                var dish1 = db.DishDatas;
                if (restaurant == null)
                {
                    // Create
                    Console.WriteLine($"Inserting {restaurantName}");
                    restaurant = db.Add(entity: new Restaurant { Name = restaurantName }).Entity;
                    db.SaveChanges();
                }

                Console.WriteLine($"Upserting dishes form {restaurant}");
                //var imperialDishesData = restaurant.DishDatas;
                foreach (var dish in dishesFromRestaurant)
                {
                    //Read
                    var dishData = db.DishDatas
                        .FirstOrDefault(d => d.Name.Equals(dish.Name) && d.RestaurantId == restaurant.RestaurantId);
                    if (dishData == null)
                    {
                        //Insert
                        restaurant.DishDatas.Add(new DishData
                        {
                            Name = dish.Name,
                            Price = dish.Price,
                            Availability = dish.Availability
                        });
                    }
                    else
                    {
                        //Update
                        dishData.Price = dish.Price;
                        dishData.Availability = dish.Availability;
                    }
                }
                db.SaveChanges();

            }
        }
    }
}
