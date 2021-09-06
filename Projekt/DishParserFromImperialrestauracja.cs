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
        public static async IAsyncEnumerable<Dish> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = $"/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[1]/div/div[2]/ul";

            //pizza
            var client = new WebClient();
            var downloadString = await client.DownloadStringTaskAsync($"https://www.imperialrestauracja.pl/restauracja/restauracja-imperial");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);
            dishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);

            foreach (var dish in dishes)
            {
                yield return dish;
            }

            //Rest of dishes
            for (var i = 2; i < 11; ++i)
            {
                baseXPath = $"/html/body/main/div/div/section/div/div/div/div[2]/div/div/div/div[{i}]/div/div[2]/ul";
                var dishContainerParent = doc.DocumentNode.SelectSingleNode(baseXPath);
                foreach (var priceNode in FindPriceNodes(dishContainerParent))
                {
                    if(priceNode != null)
                    {
                        var price = priceNode.InnerText.Trim('&', 'n', 'b', 's', 'p', ';', 'z', 'ł', '\n', ' ');
                        var dPrice = Decimal.Parse(price, new CultureInfo("pl-PL"));
                        var name = FindName(priceNode);
                        var availability = FindAvailability(priceNode);
                        Dish localDish = new Dish
                        {
                            Name = name,
                            Price = dPrice,
                            Availability = availability
                        };
                        yield return localDish;
                    }
                }
            }
        }
        public static IEnumerable<HtmlNode> FindPriceNodes(HtmlNode dishContainerParent)
        {
            var dishContainers = dishContainerParent.ChildNodes;
            foreach(var container in dishContainers)
            {
                var priceNode = container?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?
                    .ElementAtOrDefault(3);
                yield return priceNode;
            }
        }
        public static HtmlNode FindPrice(HtmlDocument doc, string priceXPath)
        {
            var priceNode = doc.DocumentNode.SelectSingleNode(priceXPath);
            return priceNode;
        }
        public static string FindName(HtmlNode price)
        {

            var dishName = price.ParentNode?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?
                .ElementAtOrDefault(3)?.InnerText.Trim();
            if (string.IsNullOrEmpty(dishName))
            {
                dishName = price.ParentNode?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?
                    .InnerText.Trim();
                if (string.IsNullOrEmpty(dishName))
                {
                    dishName = price.ParentNode?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(1)?.ChildNodes?.ElementAtOrDefault(3)?.InnerText?.Trim();
                }
            }
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
}
