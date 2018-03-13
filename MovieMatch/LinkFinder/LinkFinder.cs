using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace MovieMatch.LinkFinder
{
    public struct LinkItem
    {
        public string Href;
    }

    public class LinkFinder
    {

        public static LinkItem SearchLinks(string file)
        {
            //List<LinkItem> AmazonLinkList = new List<LinkItem>();
            LinkItem AmazonLink = new LinkItem();

            // 1.
            // Find all matches in file.

            //find the div that contains "info table-cell"
            MatchCollection divMatch = Regex.Matches(file, @"^(info table-cell)");

            //loop through each div match (should be just 1)
            foreach(Match d in divMatch)
            {
                //Find all anchor tag matches within div
                MatchCollection findAnchor = Regex.Matches(file, @"(<a.*?>*?</a>)",RegexOptions.Singleline);

                //loop inside matching anchor tags to get html
                foreach(Match a in findAnchor)
                {
                    string value = a.Groups[1].Value;

                    //get href attribute
                    Match findHref = Regex.Match(value, @"href=", RegexOptions.Singleline);
                    if (findHref.Success)
                    {
                        AmazonLink.Href = findHref.Groups[1].Value;
                    }
                }
            }
            return AmazonLink;
        }
    }
}