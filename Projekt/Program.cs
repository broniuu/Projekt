using System;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Globalization;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace consoleasync
{
    class Dish
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Status Availability { get; set; }
    }

    enum Status
    {
        unavalible,
        temporarilyUnavailable,
        avalible,
        avalibleAtSelectedTimes
    }

    //nowy parser
    class DishComparer : IEqualityComparer<Dish>
    {
        public bool Equals(Dish x, Dish y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Dish obj)
        {
            return obj.Name.GetHashCode();
        }
    }
    class DishParserGeneric
    {
        public static IEnumerable<Dish> Parse(
            HtmlDocument doc, 
            string basePath, 
            Func<HtmlNode, string> findName, 
            Func<HtmlNode, Status> findAvailability)
        {
            var dishContainer = doc.DocumentNode.SelectSingleNode(basePath);
            var prices = FindPrices(dishContainer);

            return prices
                .Select(p => CreateDish(p,findName,findAvailability))
                .Where(d => d != null)
                .Distinct(new DishComparer());

        }

        private static Dish CreateDish(HtmlNode price, Func<HtmlNode, string> findName, Func<HtmlNode, Status> findAvailability)
        {
            var sPrice = price.InnerText.Trim().Trim(' ', 'z', 'ł');
            var dPrice = Decimal.Parse(sPrice, new CultureInfo("pl-PL"));
            var name = findName(price);
            var availability = findAvailability(price);
            if (!string.IsNullOrWhiteSpace(name) && dPrice != 0)
            {
                var dish = new Dish { Name = name, Price = dPrice, Availability = availability };
                return dish;

            }
            return null;
        }
        private static IEnumerable<HtmlNode> FindPrices(HtmlNode dishContainer)
        {
            var children = dishContainer.ChildNodes;
            if (children == null || children.Count == 0)
                return Enumerable.Empty<HtmlNode>();
            var priceElements = children.Where(e => IsPrice(e.InnerText.Trim())); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindPrices(c)); // rekurencja i scalanie kolekcji
            return priceElements.Concat(recursivePriceElements); // zwracanie scalonej wersji kolekcji nodeów z cenami
        }

        private static bool IsPrice(string text)
        {
            return Regex.IsMatch(text, @"^\d+[,.]\d\d ?zł$");
        }
    }

    //--------------------------------- Klitka u witka -------------------------------
    class DishParserFromKlitkaUWitka
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";
            //Pizze
            for (var i=1; i<4; ++i)
            {
                var client1 = new WebClient();
                var downloadString1 = await client1.DownloadStringTaskAsync($"https://klitkauwitka.pl/food/category/2016/pizza/page-{i}");
                var doc1 = new HtmlDocument();
                doc1.LoadHtml(downloadString1);
                var localDishes = DishParserGeneric.Parse(doc1, baseXPath, FindName, FindAvailability);
                dishes = (i == 1 ? localDishes : dishes.Union(localDishes));
            }

            //Sałatki
            var client2 = new WebClient();
            var downloadString2 = await client2.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2246/salatki");
            var doc2 = new HtmlDocument();
            doc2.LoadHtml(downloadString2);
            dishes = dishes.Union(DishParserGeneric.Parse(doc2, baseXPath, FindName, FindAvailability));

            //Zapiekanki
            var client3 = new WebClient();
            var downloadString3 = await client3.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/1287/zapiekanki/page-1");
            var doc3 = new HtmlDocument();
            doc3.LoadHtml(downloadString3);
            dishes = dishes.Union(DishParserGeneric.Parse(doc3, baseXPath, FindName, FindAvailability));

            //Sosy do Pizzy
            var client4 = new WebClient();
            var downloadString4 = await client4.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/1288/sosy-do-pizzy");
            var doc4 = new HtmlDocument();
            doc4.LoadHtml(downloadString4);
            dishes = dishes.Union(DishParserGeneric.Parse(doc4, baseXPath, FindName, FindAvailability));

            //Zimne napoje
            var client5 = new WebClient();
            var downloadString5 = await client5.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2119/zimne-napoje");
            var doc5 = new HtmlDocument();
            doc5.LoadHtml(downloadString5);
            dishes = dishes.Union(DishParserGeneric.Parse(doc5, baseXPath, FindName, FindAvailability));

            //Wszystkie 
            var client6 = new WebClient();
            var downloadString6 = await client6.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/0/page-1");
            var doc6 = new HtmlDocument();
            doc6.LoadHtml(downloadString6);
            dishes = dishes.Union(DishParserGeneric.Parse(doc6, baseXPath, FindName, FindAvailability));

            return dishes;
        }

        private static Status FindAvailability(HtmlNode price)
        {
            var availability = Status.avalible;
            var sDishAvailability = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            switch (sDishAvailability)
            {
                case "Niedostępne":
                    availability = Status.unavalible;
                    break;
                case "Chwilowo niedostępne":
                    availability = Status.temporarilyUnavailable;
                    break;
                case "Dostępne tylko w określonych godzinach":
                    availability = Status.avalibleAtSelectedTimes;
                    break;
            }
            return availability;
        }

        private static string FindName(HtmlNode price)
        {           
            var dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            switch (dishName)
            {
                case "Dostępne tylko w określonych godzinach":
                case "Niedostępne":
                case "Promocja!":
                case "Chwilowo niedostępne":
                    dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.ElementAtOrDefault(1)?
                        .ChildNodes.FirstOrDefault().InnerText;
                    if (string.IsNullOrEmpty(dishName))
                    {
                        dishName = price?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?
                            .ChildNodes?.FirstOrDefault()?.InnerText;
                        if (string.IsNullOrEmpty(dishName))
                        {
                            dishName = price?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.InnerText;
                        }
                        //console.writeline(dishname);
                    }
                    break;
            }
            var sPrice = price.InnerText;
            dishName = dishName?.Replace(sPrice, string.Empty);
            dishName = HttpUtility.HtmlDecode(dishName);
            if (!string.IsNullOrWhiteSpace(dishName))
            {
                return dishName == "20 cm: "? string.Empty : dishName;
            }
            return string.Empty;
        }
    }

    //---------------------------------IMPERIAL RESTAURACJA-------------------------------
    class DishParserFromImperialrestauracja
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = "/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul";

            //pizze
            var client1 = new WebClient();
            var downloadString1 = await client1.DownloadStringTaskAsync($"https://www.imperialrestauracja.pl/restauracja/restauracja-imperial#menu-pizze");
            var doc1 = new HtmlDocument();
            doc1.LoadHtml(downloadString1);
            dishes = DishParserGeneric.Parse(doc1, baseXPath, FindName, FindAvailability);

            var client2 = new WebClient();
            var downloadString2 = await client2.DownloadStringTaskAsync($"https://www.imperialrestauracja.pl/restauracja/restauracja-imperial#menu-pizze");
            var doc2 = new HtmlDocument();
            doc2.LoadHtml(downloadString1);
            dishes = DishParserGeneric.Parse(doc2, baseXPath, FindName, FindAvailability);

            return dishes;
        }
        public static string FindName(HtmlNode price)
        {

            var dishName = price.ParentNode?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?
                .ElementAtOrDefault(3)?.InnerText.Trim();
            if (string.IsNullOrEmpty(dishName))
            {
                dishName = price.ParentNode?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?
                    .InnerText.Trim();
            }

            // /html[1]/body[1]/main[1]/div[1]/div[1]/section[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/ul[1]/li[1]/div[1]/div[1]/div[3]
            // /html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul/li[1]/div/div/div[4]/div/button/text()[1]
            return dishName;
        }

        public static Status FindAvailability(HtmlNode price)
        {
            var availability = Status.avalible;
            var sAvailability = price.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes.FirstOrDefault()?.ChildNodes?.
                FirstOrDefault()?.ChildNodes.FirstOrDefault()?.InnerText.Trim();
            switch (sAvailability)
            {
                case "Niedostępne":
                    availability = Status.unavalible;
                    break;
                case "Chwilowo Niedostępne":
                    availability = Status.temporarilyUnavailable;
                    break;
            }
            return availability;
        }
    }
    class DishParserFromApollo
    {

    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World with C# 9.0!");
            //var dishesFromKlitkaUWitka = await DishParserFromKlitkaUWitka.ParseFromKlitkaUWitka();
            Console.WriteLine("--------------------------------------------------");
            //foreach (var dish in dishesFromKlitkaUWitka)
            //{
            //    Console.WriteLine(dish.Name);
            //    Console.WriteLine(dish.Price);
            //    Console.WriteLine(dish.Availability);
            //}
            //var client = new WebClient();
            //var downloadString1 = await client.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2016/pizza");
            //var doc = new HtmlDocument();
            //doc.LoadHtml(downloadString1);
            //var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";
            //var dishesFromKlitkaUWitka = DishParserGeneric.Parse(doc, baseXPath);
            
            // Klitka U Witka
            //var dishesFromKiltkaUWitka = await DishParserFromKlitkaUWitka.FindDishes();
            //Console.WriteLine(string.Join(Environment.NewLine, dishesFromKiltkaUWitka.Select(d => $"{d.Name} - {d.Price} - {d.Availability}")));
            
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();

            // Imperial Restauracja
            var dishesFromImperialrestauracja = await DishParserFromImperialrestauracja.FindDishes();
            Console.WriteLine(string.Join(Environment.NewLine, dishesFromImperialrestauracja.Select(d => $"{d.Price} - {d.Name} - {d.Availability}")));
        }
    }
}
