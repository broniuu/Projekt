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
        avalible
    }
    //---------------------------------IMPERIAL RESTAURACJA-------------------------------
    class DishParserFromImperialrestauracja
    {
        public static Dish FindDishPropertiesFromImperialrestauracja(HtmlDocument doc, string nameXPath, string priceXPath, string availabiltyXPath)
        {
            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode($"{nameXPath}");
            if (nameNode == null)
            {
                return null;
            }
            var name = nameNode.InnerText.Trim();
            //Console.WriteLine(name);
            var price = doc.DocumentNode.SelectSingleNode($"{priceXPath}").InnerText.Trim().TrimEnd('z', 'ł', ' ');
            decimal dprice = Decimal.Parse(price, new CultureInfo("pl-PL"));
            //Console.WriteLine(dprice);
            string availability;
            Status sAvailability = Status.avalible;
            HtmlNode availavilityNode = doc.DocumentNode.SelectSingleNode($"{availabiltyXPath}");
            if (availavilityNode != null)
            {
                availability = availavilityNode.InnerText.Trim();
                sAvailability = Status.temporarilyUnavailable;
            }
            else
            {
                var availabiltyXPathLength = availabiltyXPath.Length - 13;
                var secondAvavailabiltyXPath = availabiltyXPath.Remove(115, 13) + "/span/span";
                availavilityNode = doc.DocumentNode.SelectSingleNode(secondAvavailabiltyXPath);
                if(availavilityNode != null)
                {
                    availability = availavilityNode.InnerText.Trim();
                    sAvailability = Status.temporarilyUnavailable;
                }
                else
                {
                    availability = "Dostępne";
                }
            }
            //Console.WriteLine(availability);
            return new Dish
            {
                Name = name,
                Price = dprice,
                Availability = sAvailability
            };
        }
        public static async Task<IEnumerable<Dish>> ParseFromImperialrestauracja()
        {
            var client = new WebClient();
            var downloadString1 = await client.DownloadStringTaskAsync("https://www.imperialrestauracja.pl/restauracja/restauracja-imperial");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString1);
            var baseXPath = "/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul";

            var dish = new List<Dish>();

            for (var i = 0; true; ++i )
            {

                Dish item = FindDishPropertiesFromImperialrestauracja(
                                    doc,
                                    $"{baseXPath}/li[{i + 1}]/div/div/div[1]/div[1]/h4",
                                    $"{baseXPath}/li[{i + 1}]/div/div/div[3]",
                                    $"{baseXPath}/li[{i + 1}]/div/div/div[1]/div[1]/div/span[2]/span");
                if(item == null)
                {
                    break;
                }
                dish.Add(item);
            }

            return dish;
        }
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
            //Console.WriteLine(sPrice);
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
            var priceElements = children.Where(e => IsPrice(e.InnerText)); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindPrices(c)); // rekurencja i scalanie kolekcji
            return priceElements.Concat(recursivePriceElements); // zwracanie scalonej wersji kolekcji nodeów z cenami
        }

        private static bool IsPrice(string text)
        {
            return Regex.IsMatch(text, @"^\d+[,.]\d\d ?zł$");
        }
    }

    //--------------------------------- Klitka u witka(niedokończona) -------------------------------
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
            }
            return availability;
        }

        private static string FindName(HtmlNode price)
        {           
            var dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            switch (dishName)
            {
                case "Niedostępne":
                case "Promocja!":
                case "Chwilowo niedostępne":
                    dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.ElementAtOrDefault(1)?
                        .ChildNodes.FirstOrDefault().InnerText;
                    if (string.IsNullOrEmpty(dishName))
                    {
                        dishName = price?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?
                            .ChildNodes?.FirstOrDefault()?.InnerText;

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
            var dishes = await DishParserFromKlitkaUWitka.FindDishes();
            Console.WriteLine(string.Join(Environment.NewLine, dishes.Select(d => $"{d.Name} - {d.Price} - {d.Availability}")));
        }
    }
}

// XPathy w sekcji pizz

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/a/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[2]/div/div[2]/div/div[1]/a/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[3]/div/div[2]/div/div[1]/a/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[4]/div/div[2]/div/div[1]/a/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[5]/div/div[2]/div/div[1]/a/h3

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div/div[2]/div/div[1]/a/h3

// XPathy w sekcji sałatek

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[2]/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[2]/div[5]/div[2]/div/div[2]/h3
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[3]/div[5]/div[2]/div/div[2]/h3

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[2]/div[2]/div/div[1]
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[2]/div[2]/div/div[2]/a/h3

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[1]
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[2]/h3

// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[2]/div[3]/div[2]/div/div[3]/div/a[2] - pizza wybór "skąponuj danie"
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[2]/div[2]/div/div[4]/div/a[2] - zapiekanka "do koszyka"
// /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[3]/div/div[2]/div/div[3]/div/a[2] - pizza "do koszyka"