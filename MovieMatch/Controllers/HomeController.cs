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
using System.Security;
using DataAccess;
using libsvm;

namespace MovieMatch.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Recommendation(string Id)
        {

            Entities ORM = new Entities();

            var UserId = Id;

            #region GenreRec
            var GenreList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.with_genres).ToList();
            var maxGenre = GenreList.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
            //what if the max is 2 genres?
            #endregion

            #region YearRec
            var YearList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.primary_release_year).ToList();
            var MaxYear = YearList.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
            //what if the max is 2 years?
            #endregion

            #region RuntimeRec
            var RuntimeList = ORM.SearchTerms.Where(x => x.Id == UserId).Select(x => x.runtime).ToList();
            int RuntimeAverage = (int)Math.Round((decimal)RuntimeList.Average());
            int RuntimeHigh = RuntimeAverage + 15;
            int RuntimeLow = RuntimeAverage - 15;
            #endregion

            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=popularity.desc&include_adult=false&include_video=false&page=1&with_genres=" + maxGenre + "&primary_release_year=" + MaxYear + "&with_runtime.lte=" +
            RuntimeHigh + "&with_runtime.gte=" + RuntimeLow);


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

        [Authorize]
        public ActionResult AddRecommendationToMovieList(MovieList movie)
        {
            if (!ModelState.IsValid)

            {
                return View("../Shared/Error"); //go to error page 
            }
            try
            {
                //1. ORM
                Entities AddMovie = new Entities();  //copy the same ORM for each of these

                if (!AddMovie.MovieLists.Any(x => x.title == movie.title))
                {
                    //2. Action: Add to MovieList table
                    AddMovie.MovieLists.Add(movie);
                    AddMovie.SaveChanges();


                    //3. stay on search results view
                    return RedirectToAction("Recommendation", new { Id = User.Identity.GetUserId() });
                }
                else
                {
                    //validates but does not warn user
                    return RedirectToAction("Recommendation", new { Id = User.Identity.GetUserId() });
                }
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Index()
        {

            return View();
        }

        [Authorize]
        public ActionResult ShowMovies()
        {
            return View("SearchResults");
        }

        [AllowAnonymous]
        public ActionResult PrivacyPolicy()
        {

            ViewBag.Message = "Privacy page";
            return View();
        }

        public ActionResult Mood()
        {
            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=revenue.desc&include_adult=false&include_video=false&page=1");


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

        public ActionResult FindMood(MovieList movie)
        {
            const string dataFilePath = @"\MovieMatch\MoodCsv\GenreList.txt";


            var dataTable = DataTable.New.ReadCsv(dataFilePath);

            List<string> x = dataTable.Rows.Select(row => row["Genre"]).ToList();

            double[] y = dataTable.Rows.Select(row => double.Parse(row["Mood"])).ToArray();

            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();

            var problemBuilder = new TextClassificationProblemBuilder();

            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());

            const int C = 1;

            var model = new C_SVC(problem, KernelHelper.LinearKernel(), C);

            string GenreId = movie.with_genres;

            Dictionary<int, string> _predictionDictionary = new Dictionary<int, string> { { -2, "Scared" }, { -1, "Sad" }, { 1, "Laugh" }, { 2, "Romance" } };

            //maybe add do,while here
            //GenreId = movie.with_genres;
            var newX = TextClassificationProblemBuilder.CreateNode(GenreId, vocabulary);

            var predictedY = model.Predict(newX);

            ViewBag.Mood = _predictionDictionary[-2];
            ViewBag.MovieTitle = movie.title;
            ViewBag.MoviePoster = movie.poster_path;



            return View();
        }

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }


    }
}