using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;

namespace MovieMatch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

        public ActionResult MovieByMood(string Mood)
        {

            Entities moviematchORM = new Entities();

            ViewBag.MovieList = moviematchORM.MovieMoods.Where(x => x.Mood == Mood).ToList();

            return View("Contact");

        }

        public ActionResult ShowMovies()
        {


            return View("SearchResults");
        }

       
    }
}