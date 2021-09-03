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
            var dishesFromImperialrestauracja = new List<Dish>();
            await foreach (var dish in DishParserFromImperialrestauracja.FindDishes())
            {
                dishesFromImperialrestauracja.Add(dish);
            }
            Console.WriteLine(string.Join(Environment.NewLine, dishesFromImperialrestauracja.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));

            var restaurantName = "Imperial Restauracja";
            UpsertDishes(dishesFromImperialrestauracja, restaurantName);
        }

        private static void UpsertDishes(List<Dish> dishesFromImperialrestauracja, string restaurantName)
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

                Console.WriteLine("Querying for a Restaurant");
                //var imperialDishesData = restaurant.DishDatas;
                foreach (var dish in dishesFromImperialrestauracja)
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

                //Console.WriteLine("Delete the restaurant");
                //db.Remove(restaurant);
                //db.SaveChanges();

                //Console.WriteLine("deleting dishes from Imperial Restauracja");
                //var dishDatas = db.DishDatas;
                //foreach (var dish in dishDatas)
                //{
                //    db.Remove(dish);
                //    db.SaveChanges();
                //}

                //Update
                //Console.WriteLine("Updating the dish and adding a post");
                //dish.Name = dishesFromImperialrestauracja.First().Name;
                //dish.Price = dishesFromImperialrestauracja.First().Price;
                //dish.Availability = (Projekt.Status)dishesFromImperialrestauracja.First().Availability;
                //db.SaveChanges();
            }
        }
    }
}
