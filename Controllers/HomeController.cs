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
            Accident model = repo.Accidents.First();
            return View(model);
        }

        public IActionResult Search()
        {
            return View();
        }

        public IActionResult Accidents()
        {
            return View();
        }

    }
}
