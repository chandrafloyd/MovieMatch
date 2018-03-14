using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace MovieMatch.LinkFinder
{


    public class LinkFinder
    {

        public static string SearchLinks(string file)
        {
            //List<LinkItem> AmazonLinkList = new List<LinkItem>();
            //string Link = "";
            string Place = "hi";
            // 1.
            // Find all matches in file.
            if (!Regex.IsMatch(file, @"(info table-cell)[\d\D]*(aiv)"""))
            {
                return Place;
            }
            else
            {
                MatchCollection divMatch = Regex.Matches(file, @"(info table-cell)[\d\D]*(aiv)""");
                MatchCollection Link = Regex.Matches(divMatch[0].Value, @"(href=)[\d\D]*(aiv)""");
                return  Link[0].Value;
            }

            //find the div that contains "info table-cell"

            


            //loop through each div match (should be just 1)
            
            //foreach (Match d in divMatch)
            //{
            //    //Find all anchor tag matches within div

            //    Link = Regex.Matches(divMatch.ToString(), @"(href=)[\d\D]*(aiv)""").ToString();


            //}
            //return Link[0].Value;
        }
    }
}