using System;
using HtmlAgilityPack;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace consoleasync
{
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
}
