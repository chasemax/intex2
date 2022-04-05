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

        public IActionResult Search()
        {
            var accident = repo.Accidents;
            return View(accident);
        }

        public IActionResult Accident(int id)
        {
            Accident accident = repo.Accidents
                .Single(x => x.Crash_ID == id);

            return View(accident);
        }

        public IActionResult Severity()
        {
            return View();
        }

    }
}
