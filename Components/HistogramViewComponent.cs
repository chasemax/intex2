using System;
using System.Collections.Generic;
using System.Linq;
using Intex2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Intex2.Components
{
    public class HistogramViewComponent :ViewComponent
    {
        private IAccidentRepo repo { get; set; }

        public HistogramViewComponent (IAccidentRepo temp)
        {
            repo = temp;
        }

        public IViewComponentResult Invoke()
        {
            Dictionary<int, Dictionary<int, DateTime>> data = new Dictionary<int, Dictionary<int, DateTime>>();

            foreach (var i in repo.Accidents)
            {
                data.Add(i.Crash_ID, new Dictionary<int, DateTime>());
                data[i.Crash_ID].Add(i.Crash_Severity_Id, i.Crash_DT);
            }

            return View(data);
        }
    }
}
