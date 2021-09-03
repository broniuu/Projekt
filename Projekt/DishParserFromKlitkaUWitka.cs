using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Projekt;

namespace consoleasync
{
    //--------------------------------- Klitka u witka -------------------------------
    class DishParserFromKlitkaUWitka
    {
        public static async Task<IEnumerable<Dish>> FindDishes()
        {
            IEnumerable<Dish> dishes = null;
            var baseXPath = "/html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul";
            // 

            var client = new WebClient();
            var downloadString = await client.DownloadStringTaskAsync($"https://www.klitkauwitka.pl/restauracja/klitka-u-witka-nowy-sacz");
            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);
            dishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);

            return dishes;
        }

        private static Status FindAvailability(HtmlNode price)
        {
            var availability = Status.avalible;
            var sDishAvailability = price?.ParentNode?.ParentNode?.ParentNode?.ChildNodes?.FirstOrDefault()?.ChildNodes?.FirstOrDefault()?.InnerText;
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[6]/div/button/text()[1]
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[5]/div/div/div[1]/div[1]/div/span
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
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[1]/div[1]/h4/text()
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[6]/div/button/text()[1]
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
