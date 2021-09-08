using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Projekt;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace consoleasync
{
    //--------------------------------- Klitka u witka -------------------------------
    class DishParserFromKlitkaUWitka
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = $"/html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul";

            //Pizza 
            var client = new WebClient();
            var downloadString = await client.DownloadStringTaskAsync($"https://www.klitkauwitka.pl/restauracja/klitka-u-witka-nowy-sacz");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);
            dishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);

            return dishes;
            //foreach (var dish in dishes)
            //{
            //    yield return dish;
            //}

        }

        private static Status FindAvailability(HtmlNode price)
        {
            
            var availability = Status.avalible;
            var sDishAvailability = ""; // Do zrobienia

            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[6]/div/button/text()[1]
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[5]/div/div/div[1]/div[1]/div/span
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[1]/div[1]/h4/text()
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

        private static string FindName(HtmlNode priceNode)
        {
            var dishNode = DishParserGeneric.FindAncestorNode(priceNode, "li");

            var nameElement = DishParserGeneric.FindNode(dishNode, "h4")?.FirstChild;

            return nameElement?.InnerText?.Trim() ?? string.Empty;

        }

    }
}
