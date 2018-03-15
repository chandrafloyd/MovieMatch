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
using System.Text;

namespace MovieMatch.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Recommendation(string Id)
        {
            try
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
                    //locate the data we want to see
                    //all results stored in a variable so that we can close the reader, make sub calls, but still access all results from original call
                    var MovieResults = MovieParse["results"];
                    ViewBag.RawResults = MovieParse["results"];

                    rd.Close();

                    //parse out the movie's id, this will be passed back into an api call to get the runtimes and genres for each movie id from RawResults
                    //this will be stored in a list
                    List<int> RuntimeResults = new List<int>();
                    List<string> GenreResults = new List<string>();
                    List<string> DisplayGenreNames = new List<string>();
                    List<string> imdb_ID_Results = new List<string>();
                    List<string> releaseYear = new List<string>();
                    List<string> AmazonLinkList = new List<string>();

                    foreach (var m in MovieResults)
                    {
                        var movieid = m["id"];
                        //http request - call the API and pass in movieid
                        HttpWebRequest requestMovieDetails = WebRequest.CreateHttp("https://api.themoviedb.org/3/movie/" + movieid + "?api_key=" + TMDBkey + "&language=en-US");

                        //browser request
                        requestMovieDetails.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

                        //http response
                        HttpWebResponse responseMovieDetails = (HttpWebResponse)requestMovieDetails.GetResponse();

                        //if we receive a response
                        if (responseMovieDetails.StatusCode == HttpStatusCode.OK)
                        {
                            //open streamreader and read output
                            StreamReader rdMovieDetails = new StreamReader(responseMovieDetails.GetResponseStream());
                            string outputMovieDetails = rdMovieDetails.ReadToEnd();

                            //parse data and store in object
                            JObject MovieDetailsParse = JObject.Parse(outputMovieDetails);

                            //get the genres -- json returns array. get names and add to list to pass to view.
                            var RawGenres = MovieDetailsParse["genres"];

                            GenreResults.Clear();
                            foreach (var genreName in RawGenres)
                            {
                                string GenreName = (string)genreName["name"];
                                GenreResults.Add(GenreName);
                            }

                            //turns our array of lists into list of strings
                            string GenreStr = string.Join(", ", GenreResults.ToArray());
                            DisplayGenreNames.Add(GenreStr);

                            //get the runtime
                            //some of the movie results do not have a runtime value listed. either a null value or a string = "null"
                            //we will store those as zero so we can exclude zeros from our recommendation runtime calculation
                            string RawRuntime = (string)MovieDetailsParse["runtime"];
                            int ActualRuntime = 0;

                            if (!int.TryParse(RawRuntime, out ActualRuntime))
                            {
                                ActualRuntime = 0;
                            }
                            else
                            {
                                ActualRuntime = int.Parse(RawRuntime);
                            }

                            RuntimeResults.Add(ActualRuntime);

                            //get imdb id for each movie
                            var imdbID = MovieDetailsParse["imdb_id"];
                            imdb_ID_Results.Add((string)imdbID);

                            //get release year
                            string movieYear = (string)MovieDetailsParse["release_date"];
                            string trimmedYear = movieYear.Substring(0, 4);

                            releaseYear.Add(trimmedYear);


                        }
                        else
                        {
                            return View("../Shared/Error");
                        }
                    }

                    //
                    foreach (string result in imdb_ID_Results)
                    {
                        string ImdbHome = "http://www.imdb.com/";

                        //add IMDB home page to list if the IMDB Id is not found
                        if (result == "" || result == null)
                        {
                            AmazonLinkList.Add(ImdbHome);
                        }
                        //if IMDB Id is listed, get URL from HTML that will direct user to the movies on Amazon
                        else
                        {
                            WebClient W = new WebClient();

                            string imdbURL = W.DownloadString("http://www.imdb.com/title/" + result + "/?ref_=fn_al_tt_1");
                            string LinkResult = LinkFinder.LinkFinder.SearchLinks(imdbURL);
                            string AmazonLink = "https://www.imdb.com/" + LinkResult;

                            //add IMDB link to the list if the Amazon link returns not found
                            if (LinkResult == "notfound")
                            {
                                string ImdbMoviePage = "http://www.imdb.com/title/" + result + "/?ref_=fn_al_tt_1";
                                AmazonLinkList.Add(ImdbMoviePage);
                            }
                            else
                            {
                                AmazonLinkList.Add(AmazonLink);
                            }
                        }
                    }

                    ViewBag.GenreResults = DisplayGenreNames;
                    ViewBag.RuntimeResults = RuntimeResults;
                    ViewBag.imdb_ID_Results = imdb_ID_Results;
                    ViewBag.trimmedYear = releaseYear;
                    ViewBag.AmazonLinks = AmazonLinkList;

                    return View("Recommendation");
                }

                else // if something is wrong (recieved a 404 or 500 error) go to this page and show the error
                {
                    return View("../Shared/Error");
                }
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }


        }

        [Authorize]
        public ActionResult AddRecommendationToMovieList(MovieList movie)
        {
            var UserId = User.Identity.GetUserId();
            if (!ModelState.IsValid)

            {
                return View("../Shared/Error"); //go to error page 
            }
            try
            {
                //1. ORM
                Entities AddMovie = new Entities();  //copy the same ORM for each of these

                if (AddMovie.MovieLists.Where(x => x.Id == UserId).Any(x => x.title != movie.title))
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
            string dataFilePath = Server.MapPath("~/MoodCsv/GenreList.txt");


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

            ViewBag.Mood = _predictionDictionary[(int)predictedY];
            ViewBag.MovieTitle = movie.title;
            ViewBag.MoviePoster = movie.poster_path;



            return View();
        }

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }


        public ActionResult FindMovieByMood(string with_genres)
        {
            var TMDBkey = ConfigurationManager.AppSettings["tmbd"];

            //keeps temp data from the stored search action
            TempData["mood"] = with_genres;
            TempData.Keep();

            //http request
            HttpWebRequest request = WebRequest.CreateHttp("https://api.themoviedb.org/3/discover/movie?api_key=" + TMDBkey +
            "&language=en-US&sort_by=popularity.desc&include_adult=false&include_video=false&page=1&with_genres=" + TempData["mood"]);

            //browser request
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

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

                //locate the data we want to see
                //all results stored in a variable so that we can close the reader, make sub calls, but still access all results from original call
                var MovieResults = MovieParse["results"];
                ViewBag.RawResults = MovieParse["results"];

                rd.Close();

                //parse out the movie's id, this will be passed back into an api call to get the runtimes and genres for each movie id from RawResults
                //this will be stored in a list
                List<int> RuntimeResults = new List<int>();
                List<string> GenreResults = new List<string>();
                List<string> DisplayGenreNames = new List<string>();
                List<string> imdb_ID_Results = new List<string>();
                List<string> releaseYear = new List<string>();
                List<string> AmazonLinkList = new List<string>();

                foreach (var m in MovieResults)
                {
                    var movieid = m["id"];
                    //http request - call the API and pass in movieid
                    HttpWebRequest requestMovieDetails = WebRequest.CreateHttp("https://api.themoviedb.org/3/movie/" + movieid + "?api_key=" + TMDBkey + "&language=en-US");

                    //browser request
                    requestMovieDetails.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

                    //http response
                    HttpWebResponse responseMovieDetails = (HttpWebResponse)requestMovieDetails.GetResponse();

                    //if we receive a response
                    if (responseMovieDetails.StatusCode == HttpStatusCode.OK)
                    {
                        //open streamreader and read output
                        StreamReader rdMovieDetails = new StreamReader(responseMovieDetails.GetResponseStream());
                        string outputMovieDetails = rdMovieDetails.ReadToEnd();

                        //parse data and store in object
                        JObject MovieDetailsParse = JObject.Parse(outputMovieDetails);

                        //get the genres -- json returns array. get names and add to list to pass to view.
                        var RawGenres = MovieDetailsParse["genres"];

                        GenreResults.Clear();
                        foreach (var genreName in RawGenres)
                        {
                            string GenreName = (string)genreName["name"];
                            GenreResults.Add(GenreName);
                        }

                        //turns our array of lists into list of strings
                        string GenreStr = string.Join(", ", GenreResults.ToArray());
                        DisplayGenreNames.Add(GenreStr);

                        //get the runtime
                        //some of the movie results do not have a runtime value listed. either a null value or a string = "null"
                        //we will store those as zero so we can exclude zeros from our recommendation runtime calculation
                        string RawRuntime = (string)MovieDetailsParse["runtime"];
                        int ActualRuntime = 0;

                        if (!int.TryParse(RawRuntime, out ActualRuntime))
                        {
                            ActualRuntime = 0;
                        }
                        else
                        {
                            ActualRuntime = int.Parse(RawRuntime);
                        }

                        RuntimeResults.Add(ActualRuntime);

                        //get imdb id for each movie
                        var imdbID = MovieDetailsParse["imdb_id"];
                        imdb_ID_Results.Add((string)imdbID);

                        //get release year
                        string movieYear = (string)MovieDetailsParse["release_date"];
                        string trimmedYear = movieYear.Substring(0, 4);

                        releaseYear.Add(trimmedYear);


                    }
                    else
                    {
                        return View("../Shared/Error");
                    }
                }

                //
                foreach (string result in imdb_ID_Results)
                {
                    string ImdbHome = "http://www.imdb.com/";

                    //add IMDB home page to list if the IMDB Id is not found
                    if (result == "" || result == null)
                    {
                        AmazonLinkList.Add(ImdbHome);
                    }
                    //if IMDB Id is listed, get URL from HTML that will direct user to the movies on Amazon
                    else
                    {
                        WebClient W = new WebClient();

                        string imdbURL = W.DownloadString("http://www.imdb.com/title/" + result + "/?ref_=fn_al_tt_1");
                        string LinkResult = LinkFinder.LinkFinder.SearchLinks(imdbURL);
                        string AmazonLink = "https://www.imdb.com/" + LinkResult;

                        //add IMDB link to the list if the Amazon link returns not found
                        if (LinkResult == "notfound")
                        {
                            string ImdbMoviePage = "http://www.imdb.com/title/" + result + "/?ref_=fn_al_tt_1";
                            AmazonLinkList.Add(ImdbMoviePage);
                        }
                        else
                        {
                            AmazonLinkList.Add(AmazonLink);
                        }
                    }
                }

                ViewBag.GenreResults = DisplayGenreNames;
                ViewBag.RuntimeResults = RuntimeResults;
                ViewBag.imdb_ID_Results = imdb_ID_Results;
                ViewBag.trimmedYear = releaseYear;
                ViewBag.AmazonLinks = AmazonLinkList;

                return View("../TMDB/MoodResults");
            }

            else // if something is wrong (recieved a 404 or 500 error) go to this page and show the error
            {
                return View("../Shared/Error");
            }
        }
   

        public bool FindMoodMethod(string g)

        {
            string dataFilePath = Server.MapPath("~/MoodCsv/GenreList.txt");


            var dataTable = DataTable.New.ReadCsv(dataFilePath);

            List<string> x = dataTable.Rows.Select(row => row["Genre"]).ToList();

            double[] y = dataTable.Rows.Select(row => double.Parse(row["Mood"])).ToArray();

            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();

            var problemBuilder = new TextClassificationProblemBuilder();

            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());

            const int C = 1;

            var model = new C_SVC(problem, KernelHelper.LinearKernel(), C);

            string GenreId = g;

            Dictionary<int, string> _predictionDictionary = new Dictionary<int, string> { { -2, "Scared" }, { -1, "Sad" }, { 1, "Laugh" }, { 2, "Romance" } };

            //maybe add do,while here
            //GenreId = movie.with_genres;
            var newX = TextClassificationProblemBuilder.CreateNode(GenreId, vocabulary);

            var predictedY = model.Predict(newX);
            if (predictedY == -2 || predictedY == -1 || predictedY == 1 || predictedY == 2)
            {
                return true;
            }
            else return false;
            // ViewBag.Mood = _predictionDictionary[-2];




        }


    }
}