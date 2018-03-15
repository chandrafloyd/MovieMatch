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
        public ActionResult GetMoviesByUser(string Id)
        {
            Entities MyMovieList = new Entities();

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == Id).ToList();

            return View();
        }


        public ActionResult ReturnToMoviesByUser(string Id)
        {
            Entities MyMovieList = new Entities();

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == Id).ToList();

            return View();
        }


        #region Sort MovieList By Table Headers

        //sort movie list by runtime
        public ActionResult SortMovieListByRuntime()
        {
            //0. store UserID in a variable so it will only sort for the logged in user
            var UserID = User.Identity.GetUserId();

            //1. ORM
            Entities MyMovieList = new Entities();

            //2. Action: Sort List by ascending

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == UserID).OrderBy(x => x.runtime).ToList();

            return View("GetMoviesByUser");
        }

        //sort movie list by year
        public ActionResult SortMovieListByYear()
        {
            //0. store UserID in a variable so it will only sort for the logged in user
            var UserID = User.Identity.GetUserId();

            //1. ORM
            Entities MyMovieList = new Entities();

            //2. Action: Sort List by ascending

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == UserID).OrderBy(x => x.runtime).ToList();

            return View("GetMoviesByUser");
        }

        //sort movie list by API rating
        public ActionResult SortMovieListByAPIRating()
        {
            //0. store UserID in a variable so it will only sort for the logged in user
            var UserID = User.Identity.GetUserId();

            //1. ORM
            Entities MyMovieList = new Entities();

            //2. Action: Sort List by ascending
            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == UserID).OrderBy(x => x.mmrating).ToList();

            return View("GetMoviesByUser");
        }

        //sort movie list by user rating
        public ActionResult SortMovieListByUserRating()
        {
            //0. store UserID in a variable so it will only sort for the logged in user
            var UserID = User.Identity.GetUserId();

            //1. ORM
            Entities MyMovieList = new Entities();

            //2. Action: Sort List by ascending
            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == UserID).OrderBy(x => x.rating).ToList();

            return View("GetMoviesByUser");
        }

        #endregion Sort MovieList By Table Headers

        //logged in user saves movie to their movielist
        public ActionResult AddToMovieList(MovieList newMovie)
        {
            var UserId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
            {
                return View("../Shared/Error");
            }
            
            try
            {
                //1. ORM
                Entities AddMovie = new Entities();

                if(AddMovie.MovieLists.Where(x => x.Id == UserId).Any(x => x.title != newMovie.title))
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

        //logged in user saves movie to their movielist
        public ActionResult AddMoodToMovieList(MovieList newMovie)
        {
            
            if (!ModelState.IsValid)
            {
                return View("../Shared/Error");
            }

            try
            {
                var UserId = User.Identity.GetUserId();
                //1. ORM
                Entities AddMovie = new Entities();

                if (AddMovie.MovieLists.Where(x => x.Id == UserId).Any(x => x.title != newMovie.title))
                {
                    //2. Action: Find the title, save to MovieList
                    AddMovie.MovieLists.Add(newMovie);
                    AddMovie.SaveChanges();

                    //3. stay Get Movies by Search view
                    return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId()  });
                }
                else
                {
                    //this validation is successful, however, it does not alert the user that they are attempting
                    //to enter a duplicate on their list
                    return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId() });
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
        public ActionResult DeleteFromMovieList(int MovieListNumber)
        {
            //1. ORM 
            Entities DeleteMovie = new Entities();

            //2. find movie by title, then delete
            DeleteMovie.MovieLists.Remove(DeleteMovie.MovieLists.Find(MovieListNumber));

            //3. Save changes
            DeleteMovie.SaveChanges();

            return RedirectToAction("GetMoviesByUser", new { Id = User.Identity.GetUserId() });
        }

        //gets selected movie for user to add rating and mark as watched
        public ActionResult UpdateMovie(int MovieListNumber)
        {
            //1. ORM 
            Entities UpdateMovie = new Entities();

            //2. find movie by title //where userID = Id?
            MovieList MovieToBeUpdated = UpdateMovie.MovieLists.Find(MovieListNumber);

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
                Movie.Entry(Movie.MovieLists.Find(updatedMovie.MovieListNumber)).
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

        //search movie list by title
        public ActionResult SearchListByTitle(string title)
        {
            var UserID = User.Identity.GetUserId();
            //1. ORM
            Entities titleSearchORM = new Entities();

            //2. Search MyList by title
            ViewBag.MovieList = titleSearchORM.MovieLists.Where(x => x.title.Contains(title) && x.Id == UserID).OrderBy(x => x.title).ToList();

            return View("../Azure/GetMoviesByUser");
        }

        //filter by genre
        public ActionResult FilterByGenre(string with_genres)
        {
            //0. store UserID in a variable so it will only sort for the logged in user
            var UserID = User.Identity.GetUserId();

            //1. ORM
            Entities MyMovieList = new Entities();

            //2. Action: Sort List by ascending

            ViewBag.MovieList = MyMovieList.MovieLists.Where(x => x.Id == UserID && x.with_genres.Contains(with_genres)).OrderBy(x => x.with_genres).ToList();

            return View("GetMoviesByUser");
        }

    }



}