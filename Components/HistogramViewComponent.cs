using System;
using System.Collections.Generic;
using System.Linq;
using Google.DataTable.Net.Wrapper.Extension;
using Google.DataTable.Net.Wrapper;
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
            IQueryable<HistogramResult> results = repo.Accidents
                .GroupBy(x => x.Crash_DT.Hour)
                .Select(x => new HistogramResult
                {
                    XVar = x.Key.ToString(),
                    YVar = x.Count(x => true)
                })
                .OrderBy(x => x.XVar);

            var googleChartData = results.ToGoogleDataTable()
                .NewColumn(new Column(ColumnType.String, "Hour"), x => x.XVar)
                .NewColumn(new Column(ColumnType.Number, "Accidents"), x => x.YVar)
                .Build()
                .GetJson();

            return View(googleChartData);
        }
    }
}
