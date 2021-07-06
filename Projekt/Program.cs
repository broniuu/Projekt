using System;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Globalization;

namespace consoleasync
{
    class Dish
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    class DishParser
    {
        //public static Dish FindDishProperties(string downloadString, string namexPath, string priceXPath)
        //{
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(downloadString);
        //    var name = doc.DocumentNode.SelectSingleNode(namexPath).InnerText.Trim();
        //    Console.WriteLine(name);
        //    var price = doc.DocumentNode.SelectSingleNode(priceXPath).InnerText.Trim();
        //    Console.WriteLine(price);
        public static async Task<Dish[]> ParseFromImperialrestauracja()
        {
            var client = new WebClient();
            var downloadString1 = await client.DownloadStringTaskAsync("https://www.imperialrestauracja.pl/restauracja/restauracja-imperial");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString1);
            var baseXPath = "/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul";
            var nameFocaccia = doc.DocumentNode.SelectSingleNode($"{baseXPath}/li[1]/div/div/div[1]/div[1]/h4").InnerText.Trim();
            Console.WriteLine(nameFocaccia);
            var priceFocaccia = doc.DocumentNode.SelectSingleNode($"{baseXPath}/li[1]/div/div/div[3]").InnerText.Trim();
            Console.WriteLine(priceFocaccia);
            var nameRomana = doc.DocumentNode.SelectSingleNode($"{baseXPath}/li[3]/div/div/div[1]/div[1]/h4").InnerText.Trim();
            Console.WriteLine(nameRomana);
            var priceRomana = doc.DocumentNode.SelectSingleNode($"{baseXPath}/li[3]/div/div/div[3]").InnerText.Trim().TrimEnd('z','ł', ' ');
            Console.WriteLine(priceRomana);
            decimal dPriceRomana = Decimal.Parse(priceRomana, style: NumberStyles.AllowDecimalPoint, provider: new CultureInfo("fr-FR"));
            Console.WriteLine(dPriceRomana);
            var dishRomania = new Dish
            {
                Name = nameRomana,
                //Price = priceRomana
            };
            var dishFocaccia = new Dish
            {
                Name = nameFocaccia,
                //Price = priceFocaccia
            };


            return new[]
            {
                dishRomania, dishFocaccia

            };
        //decimal - typ zmiennych w których będą ceny
        //Decimal.Parse - parsowanie ceny
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World with C# 9.0!");
            var dishesFromImperialrestauracja = await DishParser.ParseFromImperialrestauracja();
            
            //var dishesFromPyszne = await DishParser.Parse("https://www.pyszne.pl");

        }
    }
}