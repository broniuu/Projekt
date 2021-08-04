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
            await foreach(var dish in DishParserFromImperialrestauracja.FindDishes())
            {
                dishesFromImperialrestauracja.Add(dish);
            }
            Console.WriteLine(string.Join(Environment.NewLine, dishesFromImperialrestauracja.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));

            using (var db = new DishContext())
            {
                // Create
                //Console.WriteLine("Inserting a dishes from Imperial Restauracja");
                //foreach (var dish in dishesFromImperialrestauracja)
                //{
                    
                //    db.Add(new DishData
                //    {
                //        Name = dish.Name,
                //        Price = dish.Price,
                //        Availability = (Projekt.Status)dish.Availability
                //    });
                //    db.SaveChanges();
                //}

                Console.WriteLine("deleting dishes from Imperial Restauracja");
                var dishDatas = db.DishDatas;
                foreach (var dish in dishDatas)
                {
                    db.Remove(dish);
                    db.SaveChanges();
                }

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
