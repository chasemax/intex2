using Intex2.Models;
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
        public IActionResult Search(string city, string county)
        {

            var accident = repo.Accidents
                .Where(x => x.City == city || city == null
                && x.County_Name == county || county == null);

            ViewBag.City = repo.Accidents.Select(x => x.City).Distinct().ToList().OrderBy(x => x);
            ViewBag.County = repo.Accidents.Select(x => x.County_Name).Distinct().ToList().OrderBy(x => x);

            return View(accident);
        }

        [HttpPost]
        public IActionResult Search(Accident a)
        {
            var accident = repo.Accidents
                .Where(x => x.City == a.City || a.City == null
                && x.County_Name == a.County_Name || a.County_Name == null);

            ViewBag.City = repo.Accidents.Select(x => x.City).Distinct().ToList().OrderBy(x => x);
            ViewBag.County = repo.Accidents.Select(x => x.County_Name).Distinct().ToList().OrderBy(x => x);

            return View(accident);
        }

        public IActionResult Severity()
        {
            return View();
        }

    }
}
