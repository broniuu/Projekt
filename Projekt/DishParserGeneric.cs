using System;
using HtmlAgilityPack;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Projekt;

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
            var dishes = prices
                .Select(p => CreateDish(p, findName, findAvailability))
                .Where(d => d != null);


            var dishGroups = dishes.GroupBy(d => d.Name);
            dishes = dishGroups.SelectMany(dg => dg.Distinct(new DishComparer()));
            dishGroups = dishes.GroupBy(d => d.Name);
            dishes = dishGroups.SelectMany(dg => AddDishSizes(dg));
            
            return dishes;
        }

        private static Dish CreateDish(HtmlNode price, Func<HtmlNode, string> findName, Func<HtmlNode, Status> findAvailability)
        {
            var sPrice = price.InnerText.Trim(' ', 'z', 'ł', '&', 'n', 'b', 's', 'p', ';', '\n');
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
            if (children == null || children.Count == 0 || IsPrice(dishContainer.InnerText.Trim('&', 'n', 'b', 's', 'p', ';', '\n', ' ')))
                return Enumerable.Empty<HtmlNode>();
            var priceElements = children.Where(e => IsPrice(e.InnerText.Trim('&', 'n', 'b', 's', 'p', ';', '\n', ' '))); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindPrices(c)); // rekurencja i scalanie kolekcji
            return priceElements.Concat(recursivePriceElements); // zwracanie scalonej wersji kolekcji nodeów z cenami
        }

        private static bool IsPrice(string text)
        {
            return Regex.IsMatch(text, @"^\d+[,.]\d\d ?zł$");
        }

        public static HtmlNode FindNode(HtmlNode node, string searchedNode)
        {
            if (node == null)
            {
                return null;
            }
            var children = node.ChildNodes;

            var nameElement = children.FindFirst(searchedNode);
            if (nameElement != null)
            {
                return nameElement;
            }
            foreach (var child in children)
            {
                var childNameElement = FindNode(child, searchedNode);
                if (childNameElement != null)
                {
                    return childNameElement;
                }
            }
            return null;
        }

        public static IEnumerable<HtmlNode> FindNodes(HtmlNode node, string searchedNode)
        {
            var children = node.ChildNodes;
            if (children == null || children.Count == 0)
                return Enumerable.Empty<HtmlNode>();
            var priceElements = children.Where(e => e.Name == searchedNode); // znajdowanie HtmlNodeów z ceną, i dołączanie ich do kolekcji
            var recursivePriceElements = children.SelectMany(c => FindNodes(c, searchedNode)); // rekurencja i scalanie kolekcji
            return priceElements.Concat(recursivePriceElements); // zwracanie scalonej wersji kolekcji nodeów z cenami
        }
        public static HtmlNode FindAncestorNode(HtmlNode node, string searchedNode)
        {
            var parent = node.ParentNode;

            if(parent.Name == searchedNode)
            {
                return parent;
            }

            return FindAncestorNode(parent, searchedNode);
        }
        private static IEnumerable<Dish> AddDishSizes(IEnumerable<Dish> dishes)
        {
            if(dishes.Count() != 3)
            {
                return dishes;
            }
            dishes = dishes.OrderBy(d => d.Price);
            dishes.ElementAt(0).Name += " mała";
            dishes.ElementAt(1).Name += " średnia";
            dishes.ElementAt(2).Name += " duża";

            return dishes;
        }
    }
}
