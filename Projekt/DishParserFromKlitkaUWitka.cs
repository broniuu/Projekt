using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Projekt;
using System;
using System.Globalization;

namespace consoleasync
{
    //--------------------------------- Klitka u witka -------------------------------
    class DishParserFromKlitkaUWitka
    {
        public static async IAsyncEnumerable<Dish> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = $"/html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul";

            //Pizza 
            var client = new WebClient();
            var downloadString = await client.DownloadStringTaskAsync($"https://www.klitkauwitka.pl/restauracja/klitka-u-witka-nowy-sacz");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);
            dishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);

            foreach (var dish in dishes)
            {
                yield return dish;
            }

            // Rest of dishes
            for (var i=2; i<5; ++i)
            {
                baseXPath = $"/html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[{i}]/div/div[2]/ul";
                var dishContainer = doc.DocumentNode.SelectSingleNode(baseXPath);
                foreach (var priceNode in FindNodes(dishContainer, "button"))
                {
                    if (priceNode != null)
                    {
                        var price = FindPrice(priceNode);
                        var name = FindName(priceNode);
                        var availability = FindAvailability(priceNode);
                        Dish localDish = new Dish
                        {
                            Name = name,
                            Price = price,
                            Availability = availability
                        };
                        yield return localDish;
                    }
                }
            }
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
            var dishNameCointaner = priceNode?.ParentNode?.ParentNode?.ParentNode?.ParentNode;

            var nameElement = FindNode(dishNameCointaner, "h4")?.FirstChild;

            return nameElement?.InnerText?.Trim() ?? string.Empty;

        }
        private static Decimal FindPrice(HtmlNode priceNode)
        {
            var sPrice = priceNode?.ChildNodes?.ElementAtOrDefault(2)?.InnerText.Trim('&', 'n', 'b', 's', 'p', ';', 'z', 'ł', '\n', ' ');
            var price = Decimal.Parse(sPrice, new CultureInfo("pl-PL"));
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[2]/div/div[2]/ul/li[1]/div/div/div[2]/div/button/text()[1]
            return price;
        }

        // aby poprawnie użyć tej funkcji musisz mieć unikalną nazwę node'a (np. "h4", "button"). Jeśli nie to musisz potem zawęzić pole poszukiwań
        private static IEnumerable<HtmlNode> FindNodes(HtmlNode dishContainer, string nameOfNode)
        {
            var children = dishContainer.ChildNodes;
            if (children == null || children.Count == 0)
                return Enumerable.Empty<HtmlNode>();
            var priceElements = children.Where(e => e.Name == nameOfNode); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindNodes(c, nameOfNode)); // rekurencja i scalanie kolekcji
            return priceElements.Concat(recursivePriceElements); // zwracanie scalonej wersji kolekcji nodeów z cenami
        }

        private static HtmlNode FindNode(HtmlNode dishNode, string nameOfNode)
        {
            if(dishNode == null)
            {
                return null;
            }
            var children = dishNode.ChildNodes;

            var nameElement = children.FindFirst(nameOfNode);
            if(nameElement != null)
            {
                return nameElement;
            }
            foreach (var child in children)
            {
                var childNameElement = FindNode(child, nameOfNode);
                if (childNameElement != null)
                {
                    return childNameElement;
                }
            }
            return null;
        }
    }
}
