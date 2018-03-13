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
            string Link = "";

            // 1.
            // Find all matches in file.

            //find the div that contains "info table-cell"
            MatchCollection divMatch = Regex.Matches(file, "(<div class=\"info table-cell\">)" + @"[\d\D]*(<\/h2>)");

            //loop through each div match (should be just 1)
            foreach (Match d in divMatch)
            {
                //Find all anchor tag matches within div

                Link = Regex.Matches(file, @"(href=)[\d\D]*" + "(\")").ToString();


            }
            return Link;
        }
    }
}