using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Configuration;
using MovieMatch.Models;
using System.Text;

namespace MovieMatch.Controllers
{
    public class TMDBController : Controller
    {
        // GET: TMDB
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchMovies()
        {
                return View();
        }

        public ActionResult GetMoviesBySearch()
        {
            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            //http request
            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=popularity.desc&include_adult=false&include_video=false&page=1&with_genres=" + TempData["genre"] + 
            "&year=" + TempData["year"] + "&with_runtime.lte=" + TempData["time"]);

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
                //ViewBag.RawPlot = MovieParse["results"]["overview"].ToList();

            
                return View("SearchResults");
            }

            else // if something is wrong (recieved a 404 or 500 error) go to this page and show the error
            {
                return View("../Shared/Error");
            }
        }



        public ActionResult GetMoviesByUser(string Email)
        {
            Entities EOrm = new Entities();

            ViewBag.MovieList = EOrm.MovieLists.Where(x => x.Email.Contains(Email)).ToList();

            return View("MovieListResults");
        }


    }



}