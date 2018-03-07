using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;


namespace MovieMatch.Controllers
{
    public class AzureController : Controller
    {
        // GET: Azure
        public ActionResult GetMoviesByUser(string Email)
        {
            Entities EOrm = new Entities();
            ViewBag.MovieList = EOrm.MovieLists.Where(x => x.Email.Contains(Email)).ToList();
            return View("MovieListResults");
        }


        public ActionResult AddToMovieList(MovieList newMovie)  //this is the data updated from the form
        {
            //0. Validation (build this last, but it's really the first step that gets executed)

            if (!ModelState.IsValid)

            {
                return View("../Shared/Error"); //go to error page 
            }

            //1. ORM
            Entities AddMovie = new Entities();  //copy the same ORM for each of these


            //2. Action: Find the title, save to MovieList
            AddMovie.MovieLists.Add(newMovie);
            AddMovie.SaveChanges();

            //3. stay on search results view
            return RedirectToAction("GetMoviesBySearch", "TMDB");
        }

        public ActionResult MovieByMood(string Mood)
        {



            Entities moviematchORM = new Entities();

            ViewBag.MovieList = moviematchORM.MovieMoods.Where(x => x.Mood == Mood).ToList();

            return View("MovieByMood");

        }


    }



}