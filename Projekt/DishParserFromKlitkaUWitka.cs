using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using System.Collections.Generic;
using System.Linq;

namespace consoleasync
{
    //--------------------------------- Klitka u witka -------------------------------
    class DishParserFromKlitkaUWitka
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = "/html/body/div[1]/section/div/div[3]/div[2]/div[2]/div[1]/div[3]/div";
            //Pizze
            for (var i=1; i<4; ++i)
            {
                var client1 = new WebClient();
                var downloadString1 = await client1.DownloadStringTaskAsync($"https://klitkauwitka.pl/food/category/2016/pizza/page-{i}");
                var doc1 = new HtmlDocument();
                doc1.LoadHtml(downloadString1);
                var localDishes = DishParserGeneric.Parse(doc1, baseXPath, FindName, FindAvailability);
                dishes = (i == 1 ? localDishes : dishes.Union(localDishes));
            }

            //Sałatki
            var client2 = new WebClient();
            var downloadString2 = await client2.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2246/salatki");
            var doc2 = new HtmlDocument();
            doc2.LoadHtml(downloadString2);
            dishes = dishes.Union(DishParserGeneric.Parse(doc2, baseXPath, FindName, FindAvailability));

            //Zapiekanki
            var client3 = new WebClient();
            var downloadString3 = await client3.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/1287/zapiekanki/page-1");
            var doc3 = new HtmlDocument();
            doc3.LoadHtml(downloadString3);
            dishes = dishes.Union(DishParserGeneric.Parse(doc3, baseXPath, FindName, FindAvailability));

            //Sosy do Pizzy
            var client4 = new WebClient();
            var downloadString4 = await client4.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/1288/sosy-do-pizzy");
            var doc4 = new HtmlDocument();
            doc4.LoadHtml(downloadString4);
            dishes = dishes.Union(DishParserGeneric.Parse(doc4, baseXPath, FindName, FindAvailability));

            //Zimne napoje
            var client5 = new WebClient();
            var downloadString5 = await client5.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/2119/zimne-napoje");
            var doc5 = new HtmlDocument();
            doc5.LoadHtml(downloadString5);
            dishes = dishes.Union(DishParserGeneric.Parse(doc5, baseXPath, FindName, FindAvailability));

            //Wszystkie 
            var client6 = new WebClient();
            var downloadString6 = await client6.DownloadStringTaskAsync("https://klitkauwitka.pl/food/category/0/page-1");
            var doc6 = new HtmlDocument();
            doc6.LoadHtml(downloadString6);
            dishes = dishes.Union(DishParserGeneric.Parse(doc6, baseXPath, FindName, FindAvailability));

            return dishes;
        }

        private static Status FindAvailability(HtmlNode price)
        {
            var availability = Status.avalible;
            var sDishAvailability = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
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

        private static string FindName(HtmlNode price)
        {           
            var dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            switch (dishName)
            {
                case "Dostępne tylko w określonych godzinach":
                case "Niedostępne":
                case "Promocja!":
                case "Chwilowo niedostępne":
                    dishName = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.ElementAtOrDefault(1)?
                        .ChildNodes.FirstOrDefault().InnerText;
                    if (string.IsNullOrEmpty(dishName))
                    {
                        dishName = price?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?
                            .ChildNodes?.FirstOrDefault()?.InnerText;
                        if (string.IsNullOrEmpty(dishName))
                        {
                            dishName = price?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.InnerText;
                        }
                        //console.writeline(dishname);
                    }
                    break;
            }
            var sPrice = price.InnerText;
            dishName = dishName?.Replace(sPrice, string.Empty);
            dishName = HttpUtility.HtmlDecode(dishName);
            if (!string.IsNullOrWhiteSpace(dishName))
            {
                return dishName == "20 cm: "? string.Empty : dishName;
            }
            return string.Empty;
        }
    }
}
