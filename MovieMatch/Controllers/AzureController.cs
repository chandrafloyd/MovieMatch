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


        public ActionResult SaveToMovieList(MovieList AddToMovieList)  //this is the data updated from the form
        {
            //0. Validation (build this last, but it's really the first step that gets executed)

            if (!ModelState.IsValid)

            //this statement checks the data validations in the model.tt/customer.cs page
            //if model is not valid, go to an error page. if it is, return the view
            {
                //if someone is trying to bypass my validation in the model, can request their ip address like this and other info to see who they are with Request.UserHostAddress()

                ////a way to show individual errors per field:
                //foreach (ModelState s in ModelState.Values)

                //{
                //    foreach (ModelError r in s.Errors)
                //    {
                //        ////individual error for each field
                //        //r.ErrorMessage
                //    }
                //}


                return View("../Shared/Error"); //go to error page 
            }

            //1. ORM

            Entities EOrm = new Entities();  //copy the same ORM for each of these


            //2. Action: Find the title, save to MovieList
            EOrm.Entry(EOrm.MovieLists.Find(AddToMovieList.title)).CurrentValues.SetValues(AddToMovieList);

            EOrm.SaveChanges();

            //3. Go to customer view (but first need to load customer data(??))
            return RedirectToAction("SearchResults");
        }




    }



}