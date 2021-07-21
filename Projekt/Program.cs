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
            Console.WriteLine(name);
            var price = doc.DocumentNode.SelectSingleNode($"{priceXPath}").InnerText.Trim().TrimEnd('z', 'ł', ' ');
            decimal dprice = Decimal.Parse(price, new CultureInfo("pl-PL"));
            Console.WriteLine(dprice);
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
            Console.WriteLine(availability);
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
        public static IEnumerable<Dish> Parse(HtmlDocument doc, string basePath, Func<HtmlNode, HtmlNode> findName)
        {


            var dishContainer = doc.DocumentNode.SelectSingleNode(basePath);
            IEnumerable<HtmlNode> prices = FindPrice(dishContainer);
            var dishes = new List<Dish>();
            foreach (var price in prices)
            {
                var sPrice = price.InnerText.Trim().Trim(' ','z','ł');
                Console.WriteLine(sPrice);
                var dPrice = Decimal.Parse(sPrice, new CultureInfo("pl-PL"));
                var dishName = findName(price);
                var item = new Dish { Name = "", Price = dPrice, Availability = Status.unavalible};

                dishes.Add(item);
            }
            Console.WriteLine("");

            return dishes;


            // szukanie nazwy po cenie dania
            //price.ParentNode.ChildNodes;
        }

        private static HtmlNode FindName(HtmlNode price)
        {
            HtmlNode child = null;
            if (price.ParentNode.ParentNode.ParentNode.ChildNodes.First().ChildNodes.Count != 0)
            {
                child = price.ParentNode.ParentNode.ParentNode.ChildNodes.First().ChildNodes.First();
                Console.WriteLine(child.InnerText);
            }


            //var newBaseNode = price.ParentNode.ParentNode.ParentNode.ChildNodes.First().ChildNodes;
            //foreach (var child in newBaseNode)
            //{
            //    Console.WriteLine(child.InnerText);
            //}

            //if (IsPrice(child.InnerText))
            //{
            //    child = child.ChildNodes.First();
            //}
            //Console.WriteLine(child.InnerText);
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/div - cena
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/a/h3 - nazwa

            // Pizza

            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/div/text()
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/div - cena
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[4]/div[2]/div/div[1]/a/h3 - nazwa

            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[2]/div/div[2]/div/div[1]/div/text()
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[2]/div/div[2]/div/div[1]/a/h3

            // sałatka

            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[2]/div
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[2]/h3
            return null;
        }

        private static IEnumerable<HtmlNode> FindPrice(HtmlNode dishContainer)
        {
            var children = dishContainer.ChildNodes;
            if (children == null || children.Count == 0)
                return Enumerable.Empty<HtmlNode>();
            var priceElements = children.Where(e => IsPrice(e.InnerText)); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindPrice(c)); // rekurencja i scalanie kolekcji
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
        private static Dish FindDishPropertiesFromKlitkaUWitka(HtmlDocument doc, string newBaseXPath, string[] nameXPath, string priceXPath, string availabilityXPath)
        {
            var name = "";
            HtmlNode nameNode = null;
            foreach (var xPath in nameXPath)
            {
                nameNode = doc.DocumentNode.SelectSingleNode($"{newBaseXPath + xPath}");
                if (nameNode != null)
                {
                    name = nameNode.InnerText.Trim();
                    break;
                }
            }
            if (nameNode == null)
            {
                return null;
            }
            Console.WriteLine(name);

            priceXPath = newBaseXPath + priceXPath;
            var priceNode = doc.DocumentNode.SelectSingleNode($"{priceXPath}");
            if (priceNode == null) // jeśli nie ma takiej ścieżki, to wtedy wykorzystuje inną ścieżkę do ceny
            {
                priceXPath = newBaseXPath + "/div[3]/div[2]/div/div[1]/div/text()";
                priceNode = doc.DocumentNode.SelectSingleNode($"{priceXPath}");
            }
            var price = priceNode.InnerText.Trim().TrimEnd('z', 'ł', ' ');
            if (price == "DOSTOSUJ!")
            {
                priceXPath = newBaseXPath + "/div[3]/div[2]/div/div[1]/div/text()";
                priceNode = doc.DocumentNode.SelectSingleNode($"{priceXPath}");
                price = priceNode.InnerText.Trim().TrimEnd('z', 'ł', ' ');
            }
            Console.WriteLine(price);
            var dprice = Decimal.Parse(price, new CultureInfo("pl-PL"));
            Console.WriteLine(dprice);

            availabilityXPath = newBaseXPath + availabilityXPath;
            var availabilityNode = doc.DocumentNode.SelectSingleNode($"{availabilityXPath}");
            var sAvailability = Status.avalible;
            if(availabilityNode != null)
            {
                var availability = availabilityNode.InnerText.Trim();
                if (availability == "CHWILOWO NIEDOSTĘPNE")
                {
                    sAvailability = Status.temporarilyUnavailable;
                }
                else if (availability == "NIEDOSTĘPNE")
                {
                    sAvailability = Status.unavalible;
                }
                else
                {
                    sAvailability = Status.avalible;
                }
            }
            Console.WriteLine(sAvailability);

            return new Dish
            {
                Name = name,
                Price = dprice,
                Availability = sAvailability
            };
        }
        public static async Task<IEnumerable<Dish>> ParseFromKlitkaUWitka()
        {
            var client = new WebClient();
            var downloadString1 = await client.DownloadStringTaskAsync("https://klitkauwitka.pl/food");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString1);
            var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";

            var dish = new List<Dish>();

            // Dział z pizzami ----------------------------------
            var restOfPizzaNameXPath = new string[2];
            restOfPizzaNameXPath[0] = "/div/div[2]/div/div[1]/a/h3";
            restOfPizzaNameXPath[1] = "/div/div[2]/div/div[2]/a/h3";
            // (sałatki)
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[1] - promocja
            // /html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div/div[1]/div[5]/div[2]/div/div[2]/h3 - nazwa

            var restOfPizzaPriceXPaths = new string[3];
            restOfPizzaPriceXPaths[0] = "/div/div[2]/div/div[2]/div[1]/b/a";
            restOfPizzaPriceXPaths[1] = "/div/div[2]/div/div[2]/div[2]/b/a";
            restOfPizzaPriceXPaths[2] = "/div/div[2]/div/div[2]/div[3]/b/a";

            var restOfPizzaAvailabilityXPath = "/div/div[2]/div/div[1]"; // nie wiadomo czy na pewno ta ścieżka

            var numbersOfPages = new List<int> { 1, 2, 3 };
            foreach(var page in numbersOfPages)
            {
                Console.WriteLine($"--------PAGE {page}--------");
                var pageAddress = $"https://klitkauwitka.pl/food/category/2016/pizza/page-{page}";
                var downloadString2 = await client.DownloadStringTaskAsync(pageAddress);
                var doc2 = new HtmlDocument();
                doc2.LoadHtml(downloadString2);
                var stop = true;
                var tableOfPizzaSizes = new string[3]{ " mała"," średnia", " duża"};
                var bufferForName = "";
                for (var i = (page == 1 ? 2 : 1); stop; ++i)
                {
                    var pizzaSizeIndex = 0;
                    foreach (var XPath in restOfPizzaPriceXPaths)
                    {
                        Dish item = FindDishPropertiesFromKlitkaUWitka(
                            doc2,
                            $"{baseXPath}/div[{i}]", // newBaseXPath
                            restOfPizzaNameXPath,
                            XPath,
                            restOfPizzaAvailabilityXPath
                        ) ;
                        if (item == null)
                        {
                            stop = false;
                            break;
                        }
                        if (i == 1 && XPath == restOfPizzaPriceXPaths.First()) // ustawienie początkowej wartości bufora
                        {
                            bufferForName = item.Name;
                        }

                        if (item.Price != dish.Last().Price && bufferForName == item.Name) // jeśli cena jest różna od poprzedniej i nazwa taka sama (można wybrać rozmiar pizzy)
                        {
                            bufferForName = item.Name;
                            item.Name += tableOfPizzaSizes[pizzaSizeIndex];
                            ++pizzaSizeIndex;
                            dish.Add(item);
                        }
                    }
                }
            }
            Console.WriteLine("---------------- SALADS ------------------");

            // Dział z sałatkami

            var downloadString3 = await client.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2246/salatki");
            var doc3 = new HtmlDocument();
            doc3.LoadHtml(downloadString3);

            var restOfSaladNameXPath = new string[2];
            restOfSaladNameXPath[0] = "/div[5]/div[2]/div/div[2]/h3";
            restOfSaladNameXPath[1] = "/div[5]/div[2]/div/div[1]/h3";

            var restOfSaladPriceXPath = "/div[5]/div[2]/div/div[2]/div";

            var restOfSaladAvailabilityXPath = "/div[5]/div[2]/div/div[1]";

            for (int i = 1; true; ++i)
            {
                Dish item = FindDishPropertiesFromKlitkaUWitka(
                    doc3,
                    $"{baseXPath}/div[{i}]", // newBaseXPath
                    restOfSaladNameXPath,
                    restOfSaladPriceXPath,
                    restOfSaladAvailabilityXPath
                );
                if (item == null)
                {
                    break;
                }
                dish.Add(item);
            }
            return dish;
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
            var client = new WebClient();
            var downloadString1 = await client.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2016/pizza");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString1);
            var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";
            var dishesFromKlitkaUWitka = DishParserGeneric.Parse(doc, baseXPath);

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