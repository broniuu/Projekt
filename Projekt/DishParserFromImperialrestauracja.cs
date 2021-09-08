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
            var baseXPath = $"/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul";

            //pizza
            var client = new WebClient();
            var downloadString = await client.DownloadStringTaskAsync($"https://www.imperialrestauracja.pl/restauracja/restauracja-imperial");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);
            dishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);

            return dishes;
        }
        public static string FindName(HtmlNode priceNode)
        {

            var dishNode = DishParserGeneric.FindAncestorNode(priceNode, "li");
            var nameElement = DishParserGeneric.FindNode(dishNode, "h4")?.FirstChild;

            return nameElement?.InnerText?.Trim() ?? string.Empty;
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
}
