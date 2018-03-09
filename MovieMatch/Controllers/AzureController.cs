using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;
using Microsoft.AspNet.Identity;
using System.Security;


namespace MovieMatch.Controllers
{
    [Authorize]
    public class AzureController : Controller
    {
        //find the movies saved to movielist for the logged in user
        //need to add authorization so that this action cannot be accessed via anonymous
        public ActionResult GetMoviesByUser(string Id)
        {
            Entities MyMovieList = new Entities();

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == Id).ToList();

            return View();
        }

        //logged in user saves movie to their movielist
        public ActionResult AddToMovieList(MovieList newMovie)
        {
            if (!ModelState.IsValid)
            {
                return View("../Shared/Error");
            }

            try
            {
                //1. ORM
                Entities AddMovie = new Entities();

                if (!AddMovie.MovieLists.Any(x => x.title == newMovie.title))
                {
                    //2. Action: Find the title, save to MovieList
                    AddMovie.MovieLists.Add(newMovie);
                    AddMovie.SaveChanges();

                    //3. stay Get Movies by Search view
                    return RedirectToAction("GetMoviesBySearch", "TMDB");
                }
                else
                {
                    //this validation is successful, however, it does not alert the user that they are attempting
                    //to enter a duplicate on their list
                    return RedirectToAction("GetMoviesBySearch", "TMDB");
                }
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        //logged in user saves movie to their movielist, from the Movies by Mood generator
        public ActionResult MoodRecToMovieList(MovieList newMovie)
        {
            if (!ModelState.IsValid)
            {
                return View("../Shared/Error");
            }
            try
            {
                //1. ORM
                Entities AddMovie = new Entities();

                if (!AddMovie.MovieLists.Any(x => x.title == newMovie.title))
                {
                    //2. Action: Find the title, save to MovieList
                    AddMovie.MovieLists.Add(newMovie);
                    AddMovie.SaveChanges();

                    //3. move to the user's movielist page
                    return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId() });
                }
                else
                {
                    //this validation is successful, however, it does not alert the user that they are attempting
                    //to enter a duplicate on their list. also, it returns to movie by mood, but the mood is lost
                    //(maybe store in temp data and pass back?) so the page does not display results until a mood is selected again.
                    return RedirectToAction("MovieByMood");

                }
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        //show recommended movies based on mood selected from drop down
        public ActionResult MovieByMood(string Mood)
        {
            Entities moviematchORM = new Entities();

            ViewBag.MovieList = moviematchORM.MovieMoods.Where(x => x.mood == Mood).ToList();

            return View("MovieByMood");
        }

        //deletes movie from movie list (javascript confirmation when link to this action is clicked)
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

        //gets selected movie for user to add rating and mark as watched
        public ActionResult UpdateMovie(string title)
        {
            //1. ORM 
            Entities UpdateMovie = new Entities();

            //2. find movie by title
            MovieList MovieToBeUpdated = UpdateMovie.MovieLists.Find(title);

            ViewBag.MovieToUpdate = MovieToBeUpdated;

            return View();
        }

        //saves updates to rating and watched y/n -- input is from drop down
        public ActionResult SaveMovieUpdate(MovieList updatedMovie)
        {
            if (!ModelState.IsValid)
            {
                return View("../Shared/Error");// error page 
            }
            try
            {
                //1. ORM
                Entities Movie = new Entities();

                //2. Find the Movie 
                Movie.Entry(Movie.MovieLists.Find(updatedMovie.title)).
                    CurrentValues.SetValues(updatedMovie);
                //3.
                Movie.SaveChanges();

                //4. back to movielist for the user
                return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId() });
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }

        }
    }



}