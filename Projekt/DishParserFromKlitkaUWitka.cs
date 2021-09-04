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
            for (var i = 1; i < 4; ++i)
            {
                var baseXPath = $"/html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[{i}]/div/div[2]/ul";
                // 

                var client = new WebClient();
                var downloadString = await client.DownloadStringTaskAsync($"https://www.klitkauwitka.pl/restauracja/klitka-u-witka-nowy-sacz");
                var doc = new HtmlDocument();
                doc.LoadHtml(downloadString);
                var localDishes = DishParserGeneric.Parse(doc, baseXPath, FindName, FindAvailability);
                dishes = (i == 1 ? localDishes : dishes.Union(localDishes));
            }
            return dishes;
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

        private static string FindName(HtmlNode price)
        {
            var dishNameCointaner = price?.ParentNode?.ParentNode?.ParentNode?.ParentNode?.ParentNode?.ParentNode;
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[6]/div/button/text()[1]
            string dishName = null;
            for (var i = 0; i < 4; ++i)
            {
                var nameSearcher = price?
                .ParentNode?
                .ChildNodes?.ElementAtOrDefault(1)?
                .ChildNodes?.ElementAtOrDefault(1)?
                .ChildNodes?.ElementAtOrDefault(i)?
                .ChildNodes?.ElementAtOrDefault(0)?
                .InnerText;
                if (nameSearcher != null)
                {
                    dishName = nameSearcher;
                }
                
            }
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[1]/div/div[2]/ul/li[2]/div/div/div[1]/div[1]/h4/text()
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[2]/div/div[2]/ul/li[1]/div/div/div[1]/div[1]/h4/text()

            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[2]/div/div[2]/ul/li[1]/div/div/div[1]/div[1]/h4/text()
            // /html/body/main/section[2]/div[2]/div/div/div/div/div/div/div[2]/div/div[2]/ul/li[1]/div/div/div[2]/div/button/text()[1]

            return dishName == null ? string.Empty : dishName.Trim();
        }

        private static string FindNameNode(HtmlNode dishNode)
        {
            var children = dishNode.ChildNodes;

            var nameElement = children.FindFirst("h4");
            if(nameElement != null)
            {
                return nameElement;
            }
            FindNameNode()
            return null;
        }
    }
}
