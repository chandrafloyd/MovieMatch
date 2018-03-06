using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieMatch.Models;

namespace MovieMatch.Controllers
{
    public class StoredSearchController : Controller
    {
        // GET: StoredSearch
        public ActionResult SaveSearchResult(SearchTerm newStoredSearch, string with_genres, string primary_release_year, string runtime)
        {
            TempData["genre"] = with_genres;
            TempData["year"] = primary_release_year;
            TempData["time"] = runtime;

            //1 ORM
            Entities SaveSearch = new Entities();

            //2 add to DB
            SaveSearch.SearchTerms.Add(newStoredSearch);
            SaveSearch.SaveChanges();

            return RedirectToAction("GetMoviesBySearch", "TMDB");
        }
    }
}