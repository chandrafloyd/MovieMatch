using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;
using Microsoft.AspNet.Identity;
using System.Configuration;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MovieMatch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Recomendation()
        {
            Entities ORM = new Entities();
            var gener = ORM.SearchTerms.Find(User.Identity.GetUserId()).with_genres.ToList();

           
            var frequency = gener.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
           var maxGener= frequency.Keys.Max();

            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=vote_average.asc&include_adult=false&include_video=false&page=1&with_genres=" + maxGener);
           

            //browser request
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";


            //request.Headers  (if needed, can request it. depends on the api documentation)

            //http response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //if we receive a response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //open streamreader and read output
                StreamReader rd = new StreamReader(response.GetResponseStream());
                string output = rd.ReadToEnd();

                //parse data and store in object
                JObject MovieParse = JObject.Parse(output);

                //locate the data we want to see. check node tree in jsonviewer
                ViewBag.RawResults = MovieParse["results"];


                return View();
            }


            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult ShowMovies()
        {
            return View("SearchResults");
        }


    }
}