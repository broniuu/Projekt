using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;
using System.Globalization;
using Projekt;

namespace consoleasync
{
    class DishParserFromImperialrestauracja
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            for (var i = 1; i < 11; ++i)
            {
                var baseXPath = $"/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[{i}]/div/div[2]/ul";

                var client = new WebClient();
                var downloadString = await client.DownloadStringTaskAsync($"https://www.imperialrestauracja.pl/restauracja/restauracja-imperial");
                var doc = new HtmlDocument();
                doc.LoadHtml(downloadString);
                dishes = i==1 
                    ?  DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability) 
                    : dishes.Union(DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability));
            }
            return dishes;
        }
        public static string FindName(HtmlNode priceNode)
        {

            var dishNode = DishParserGeneric.FindAncestorNode(priceNode, "li");
            var nameElement = DishParserGeneric.FindNode(dishNode, "h4")?.FirstChild;

            return nameElement?.InnerText?.Trim() ?? string.Empty;
        }

        public static Status FindAvailability(HtmlNode priceNode)
        {
            var availability = Status.avalible;
            var dishNode = DishParserGeneric.FindAncestorNode(priceNode, "li");
            var availabilityNode = DishParserGeneric.FindNode(dishNode, "span");
            var sDishAvailability = availabilityNode.InnerText.Trim();
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
    }
}
