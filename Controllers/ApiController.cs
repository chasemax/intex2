using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.DataTable.Net.Wrapper.Extension;
using Google.DataTable.Net.Wrapper;
using Intex2.Models;

namespace Intex2.Controllers
{
    public class ApiController : Controller
    {
        private IAccidentRepo repo { get; set; }

        public ApiController(IAccidentRepo t) => repo = t;

        public IActionResult HistogramData()
        {
            IEnumerable<Accident> allAccidents = repo.Accidents.ToList();
            IEnumerable<int> allHours = Enumerable.Range(0, 24);
            IEnumerable<DataResult> histogramData = new List<DataResult>();

            foreach (int hour in allHours)
            {
                DataResult hr = new DataResult();
                hr.XVar = hour.ToString();
                hr.YVar = allAccidents.Where(x => x.Crash_DT.Hour == hour).Count();
                histogramData = histogramData.Append(hr);
            }

            var googleChartData = histogramData.ToGoogleDataTable()
                .NewColumn(new Column(ColumnType.String, "Hour"), x => x.XVar)
                .NewColumn(new Column(ColumnType.Number, "Accidents"), x => x.YVar)
                .Build()
                .GetJson();

            return Content(googleChartData);
        }

        public IActionResult PieChartData()
        {
            IEnumerable<Accident> allSevereAccidents = repo.Accidents.Where(x => x.Crash_Severity_Id >= 3).ToList();
            IEnumerable<DataResult> pieChartData = new List<DataResult>();
            pieChartData = pieChartData.Append(new DataResult
            {
                XVar = "DUI",
                YVar = allSevereAccidents.Where(x => x.DUI).Count()
            });
            pieChartData = pieChartData.Append(new DataResult
            {
                XVar = "Non-DUI",
                YVar = allSevereAccidents.Where(x => !x.DUI).Count()
            });

            var googleChartData = pieChartData.ToGoogleDataTable()
                .NewColumn(new Column(ColumnType.String, "DUI"), x => x.XVar)
                .NewColumn(new Column(ColumnType.Number, "Accidents"), x => x.YVar)
                .Build()
                .GetJson();

            return Content(googleChartData);
        }
    }
}
