using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
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
        private IConfiguration Configuration { get; set; }
        private InferenceSession iSession { get; set; }

        public HomeController(IAccidentRepo temp, IConfiguration config, InferenceSession sess)
        {
            repo = temp;
            Configuration = config;
            iSession = sess;
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
        public IActionResult Search(int pageNum = 1, string county="All", string city="All", string severity="All", string searchText="")
        {
            int pageSize = 20;

            var allAccidents = repo.Accidents
                .Where(x => x.Crash_ID.ToString().Contains(searchText) || x.Main_Road_Name.Contains(searchText) || searchText == "")
                .Where(x => x.City == city || city == "All")
                .Where(x => x.County_Name == county || county == "All")
                .Where(x => x.Crash_Severity_Id.ToString() == severity || severity == "All");

            var accidents = allAccidents
                .OrderByDescending(x => x.Crash_DT)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize);

            IEnumerable<string> counties = allAccidents.Select(x => x.County_Name).Distinct().ToList().Append("All").OrderBy(x => x);
            IEnumerable<string> cities = allAccidents.Select(x => x.City).Distinct().ToList().Append("All").OrderBy(x => x);
            IEnumerable<string> severities = allAccidents.Select(x => x.Crash_Severity_Id.ToString()).Distinct().ToList().Append("All").OrderByDescending(x => x);

            var cityFilterQueries = new List<KeyValuePair<string, Dictionary<string, string>>>();
            var countyFilterQueries = new List<KeyValuePair<string, Dictionary<string, string>>>();
            var severityFilterQueries = new List<KeyValuePair<string, Dictionary<string, string>>>();

            foreach (string current_city in cities)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    { "city", current_city },
                    { "county", county },
                    { "severity", severity },
                    { "pageNum", pageNum.ToString() },
                    { "searchText", searchText }
                };
                cityFilterQueries.Add(new KeyValuePair<string, Dictionary<string, string>>(current_city, queryParams));
            }

            foreach (string current_county in counties)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    { "city", city },
                    { "county", current_county },
                    { "severity", severity },
                    { "pageNum", pageNum.ToString() },
                    { "searchText", searchText }
                };
                countyFilterQueries.Add(new KeyValuePair<string, Dictionary<string, string>>(current_county, queryParams));
            }

            foreach (string current_severity in severities)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    { "city", city },
                    { "county", county },
                    { "severity", current_severity },
                    { "pageNum", pageNum.ToString() },
                    { "searchText", searchText }
                };
                severityFilterQueries.Add(new KeyValuePair<string, Dictionary<string, string>>(current_severity, queryParams));
            }

            // 1. Filter all records that we have, apply pagination skip and take

            var pageInfo = new PageInfo
            {
                TotalNumCrashes = allAccidents.Count(),
                CrashesPerPage = pageSize,
                CurrentPage = pageNum,
                CityFilterQueries = cityFilterQueries,
                CountyFilterQueries = countyFilterQueries,
                SeverityFilterQueries = severityFilterQueries,
                SelectedCity = city,
                SelectedCounty = county,
                SelectedSeverity = severity,
                CurrentSearchQuery = searchText
            };

            var viewmodel = new AccidentsViewModel
            {
                Accidents = accidents,
                PageInfo = pageInfo,
            };

            return View(viewmodel);
        }

        [HttpPost]
        public IActionResult Search()
        {
            string searchText = Request.Form["searchText"];
            Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    { "searchText", searchText }
                };
            return RedirectToAction("Search", queryParams);
        }

        public IActionResult Accident(int id)
        {
            var accident = repo.Accidents
                .Single(x => x.Crash_ID == id);

            ViewBag.mapsUrl = "https://maps.googleapis.com/maps/api/staticmap?center="
                + accident.Latitude.ToString() + ","
                + accident.Longitude.ToString()
                + "&zoom=16&size=500x400&maptype=roadmap&markers=color:red%7C"
                + accident.Latitude.ToString() + ","
                + accident.Longitude.ToString()
                + "&key=" + Configuration["GoogleAPIKey"];

            float[] dataForModel = new float[]
            {
                (float)accident.MilePoint, 
                accident.Intersection_Related ? 1 : 0, 
                accident.Teenage_Driver_Involved ? 1 : 0,
                accident.Older_Driver_Involved ? 1 : 0, 
                accident.Night_Dark_Condition ? 1 : 0, 
                accident.Single_Vehicle ? 1 : 0,
                accident.Roadway_Departure ? 1 : 0, 
                accident.Route != 15 && accident.Route != 89 ? 1 : 0, 
                accident.Main_Road_Name != "I-15" ? 1 : 0,
                accident.City == "OUTSIDE CITY LIMITS" ? 1 : 0, 
                accident.County_Name != "SALT LAKE" && accident.County_Name != "WEBER" && accident.County_Name != "DAVIS" && accident.County_Name != "UTAH"? 1 : 0,
                accident.County_Name == "SALT LAKE" ? 1 : 0, 
                accident.County_Name == "UTAH" ? 1 : 0,
                (float)accident.Latitude, 
                (float)accident.Longitude,
                (float)accident.Crash_DT.Hour, 
                (accident.Crash_DT - new DateTime(2016, 1, 1)).Days
            };

            int[] dimensions = new int[] { 1, dataForModel.Length };

            DenseTensor<float> tensorForModel = new DenseTensor<float>(dataForModel, dimensions);

            var result = iSession.Run(new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor<float>("float_input", tensorForModel)
            });

            Tensor<long> classification = result.First().AsTensor<long>();
            int predictedSeverity = (int)classification.First();
            result.Dispose();

            ViewBag.predictedSeverity = predictedSeverity;
            
            return View(accident);
        }

        public IActionResult Severity()
        {
            return View();
        }

    }
}
