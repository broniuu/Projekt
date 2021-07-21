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

    class DishParserGeneric
    {
        public static IEnumerable<Dish> Parse(HtmlDocument doc, string basePath, Func<HtmlNode, string> findName)
        {
            var dishContainer = doc.DocumentNode.SelectSingleNode(basePath);
            var prices = FindPrices(dishContainer);
            var dishes = new List<Dish>();
            foreach (var price in prices)
            {
                var sPrice = price.InnerText.Trim().Trim(' ','z','ł');
                //Console.WriteLine(sPrice);
                var dPrice = Decimal.Parse(sPrice, new CultureInfo("pl-PL"));
                var name = findName(price);
                if (!string.IsNullOrWhiteSpace(name) && dPrice != 0)
                {
                    var item = new Dish { Name = name, Price = dPrice, Availability = Status.avalible };

                    dishes.Add(item);
                }
            }
            //Console.WriteLine("");

            return dishes;
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
            for (var i=1; i<4; ++i)
            {
                var client = new WebClient();
                var downloadString1 = await client.DownloadStringTaskAsync($"https://klitkauwitka.pl/food/category/2016/pizza/page-{i}");

                var doc = new HtmlDocument();
                doc.LoadHtml(downloadString1);
                var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";
                var localDishes = DishParserGeneric.Parse(doc, baseXPath, FindName);
                dishes = (i == 1 ? localDishes : dishes.Union(localDishes));
                //dishes = dishes.Union(localDishes);
            }

            return dishes;
        }


        private static string FindName(HtmlNode price)
        {
            var dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            if (!string.IsNullOrWhiteSpace(dishName))
            {
                //Console.WriteLine(dishName);
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