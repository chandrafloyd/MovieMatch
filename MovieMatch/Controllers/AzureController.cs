using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;
using Microsoft.AspNet.Identity;


namespace MovieMatch.Controllers
{
    public class AzureController : Controller
    {
        // GET: Azure
        public ActionResult GetMoviesByUser(string Id)
        {
            Entities MyMovieList = new Entities();

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == Id).ToList();

            return View();
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

        public ActionResult DeleteFromMovieList(string title)
        {
            //1. ORM 
            Entities DeleteMovie = new Entities();

            //2. find movie by title, then delete
            DeleteMovie.MovieLists.Remove(DeleteMovie.MovieLists.Find(title));

            //3. Save changes
            DeleteMovie.SaveChanges();

            return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId() });
        }

        public ActionResult UpdateMovie(string title)
        {
            //1. ORM 
            Entities UpdateMovie = new Entities();

            //2. find movie by title
            UpdateMovie.MovieLists.Find(title);

            ViewBag.MovieTitle = $"Rate {title}";

            return View();
        }


    }



}