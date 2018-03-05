using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Configuration;

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
            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];
            //http request
            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/movie/550?api_key="+ TMDBkey);

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
                var Title = MovieParse["original_title"];

                ViewBag.Message = $"The title is: {Title}";

                return View();
            }

            else // if something is wrong (recieved a 404 or 500 error) go to this page and show the error
            {
                return View("../Shared/Error");
            }

        }

        public ActionResult GetMoviesBySearch(string MovieTitle)
        {
            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];
            //http request
            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
                "&language=en-US&sort_by=popularity.asc&include_adult=false&include_video=false&page=1&with_keywords=" + MovieTitle);
            //https://api.themoviedb.org/3/discover/movie?api_key=b4b0128c67aa31fa68e47bb1b58a61d5&certification_country=US&certification.lte=G&sort_by=popularity.desc


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
                var Title = MovieParse["original_title"];

                ViewBag.SearchMessage = $"The title is: {Title}";

                return View("SearchMovies");
            }

            else // if something is wrong (recieved a 404 or 500 error) go to this page and show the error
            {
                return View("../Shared/Error");
            }
        }
    }
}