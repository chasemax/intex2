using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Controllers
{
    public class HomeController : Controller
    {
        private IAccidentRepo repo { get; set; }

        public HomeController(IAccidentRepo temp)
        {
            repo = temp;
        }

        public IActionResult Index()
        {
            return View();
        }

        //int id = 0, string city = "", string county = "", DateTime? mindate = null, DateTime? maxdate = null, int severity = 0,
        //    bool bicinv = false, bool mvinv = false, bool disdriv = false, bool domanimal = false, bool drowsy = false, bool dui = false, bool restraint = false,
        //    bool intersection = false, bool motorcycle = false, bool night = false, bool olddriver = false, bool rollover = false, bool pedestrian = false,
        //    bool roaddep = false, bool singleveh = false, bool teendriver = false, bool unrestrained = false, bool wildanimal = false, bool workzoneß = false

        [HttpGet]
        public IActionResult Search(int pageNum = 1)
        {
            int pageSize = 20;

            var x = new AccidentsViewModel
            {
                Accidents = repo.Accidents
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),

                PageInfo = new PageInfo
                {
                    TotalNumCrashes = repo.Accidents.Count(),
                    CrashesPerPage = pageSize,
                    CurrentPage = pageNum
                }
            };

            ViewBag.City = repo.Accidents.Select(x => x.City).Distinct().ToList().OrderBy(x => x);
            ViewBag.County = repo.Accidents.Select(x => x.County_Name).Distinct().ToList().OrderBy(x => x);

            return View(x);
        }

        public IActionResult Accident(int id)
        {
            var accident = repo.Accidents
                .Single(x => x.Crash_ID == id);

            return View(accident);
        }

        public IActionResult Severity()
        {
            return View();
        }

    }
}
