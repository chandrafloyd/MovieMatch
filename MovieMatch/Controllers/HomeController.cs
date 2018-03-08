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
        public ActionResult Recommendation()
        {

            Entities ORM = new Entities();

            var UserId = User.Identity.GetUserId();

            #region GenreRec
            var GenreList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.with_genres).ToList();
            var frequency = GenreList.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var maxGener = frequency.Keys.Max();
            #endregion

            #region YearRec
            var YearList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.primary_release_year).ToList();
            var YearFreq = YearList.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var MaxYear = YearFreq.Keys.Max();
            #endregion

            #region RuntimeRec
            var RuntimeList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.runtime).ToList();
            int RuntimeAverage = (int)Math.Round((decimal)RuntimeList.Average());
            //consider taking this average and creating a min and max runtime (avg +/- 20 min) and pass both
            //into api call with_runtime.lte = max, with_runtime.gte = min.
            #endregion

            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=popularity.desc&include_adult=false&include_video=false&page=1&with_genres=" + maxGener + "&primary_release_year=" + MaxYear + "&with_runtime.lte=" + RuntimeAverage);


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
            else
            {
                return View("../Shared/Error");
            }


        }


        public ActionResult AddRecommendationToMovieList(MovieList movie)
        {
            if (!ModelState.IsValid)

            {
                return View("../Shared/Error"); //go to error page 
            }

            //1. ORM
            Entities AddMovie = new Entities();  //copy the same ORM for each of these


            //2. Action: Add to MovieList table
            AddMovie.MovieLists.Add(movie);
            AddMovie.SaveChanges();

            //3. stay on search results view
            return RedirectToAction("Recommendation");
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