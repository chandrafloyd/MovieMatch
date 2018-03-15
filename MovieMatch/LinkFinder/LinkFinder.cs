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
            string Place = "notfound";
            
            //finds matching text in imdb page where Amazon link is located

            //if the link does not exist, return "notfound" -- use this in if/else in GetMoviesBySearch
            if (!Regex.IsMatch(file, @"(info table-cell)[\d\D]*(aiv)"""))
            {
                return Place;
            }
            else
            {
                //if there is amatch, grb the text within the Href tag
                MatchCollection divMatch = Regex.Matches(file, @"(info table-cell)[\d\D]*(aiv)""");
                MatchCollection Link = Regex.Matches(divMatch[0].Value, @"(of)[\d\D]*(aiv)""");
                return  Link[0].Value;
            }
        }
    }
}