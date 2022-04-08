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
        private InferenceSession accidentSession { get; set; }
        private InferenceSession calculatorSession { get; set; }

        public HomeController(IAccidentRepo temp, IConfiguration config)
        {
            repo = temp;
            Configuration = config;
            accidentSession = new InferenceSession("crashlessutah.onnx"); // Insert file path here
            calculatorSession = new InferenceSession("crashlessutah_bool.onnx"); // Insert file path here
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
                    { "pageNum", "1".ToString() },
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
                    { "pageNum", "1".ToString() },
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
                    { "pageNum", "1".ToString() },
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
                + "&zoom=16&size=450x350&maptype=roadmap&markers=color:red%7C"
                + accident.Latitude.ToString() + ","
                + accident.Longitude.ToString()
                + "&key=" + Configuration["GoogleAPIKey"];

            float[] dataForModel = new float[]
            {
                (float)accident.Latitude, 
                (float)accident.Longitude,
                (float)accident.Crash_DT.Hour, 
                (accident.Crash_DT - new DateTime(2016, 1, 1)).Days
            };

            int[] dimensions = new int[] { 1, dataForModel.Length };

            DenseTensor<float> tensorForModel = new DenseTensor<float>(dataForModel, dimensions);

            var result = accidentSession.Run(new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor<float>("float_input", tensorForModel)
            });

            Tensor<string> classification = result.First().AsTensor<string>();
            string predictedSeverity = classification.First();
            result.Dispose();

            ViewBag.predictedSeverity = predictedSeverity;
            
            return View(accident);
        }

        [HttpGet]
        public IActionResult Severity()
        {
            return View(new PredictionModel());
        }

        [HttpPost]
        public IActionResult Severity(PredictionModel p)
        {
            float[] dataForModel = new float[]
            {
                p.Work_Zone_Related ? 1 : 0,
                p.Pedestrian_Involved ? 1 : 0,
                p.Bicyclist_Involved ? 1 : 0,
                p.Motorcycle_Involved ? 1 : 0,
                p.Improper_Restraint ? 1 : 0,
                p.Unrestrained ? 1 : 0,
                p.DUI ? 1 : 0,
                p.Intersection_Related ? 1 : 0,
                p.Wild_Animal_Related ? 1 : 0,
                p.Domestic_Animal_Related ? 1 : 0,
                p.Overturn_Rollover ? 1 : 0,
                p.Commercial_Motor_Veh_Involved ? 1 : 0,
                p.Teenage_Driver_Involved ? 1 : 0,
                p.Older_Driver_Involved ? 1 : 0,
                p.Night_Dark_Condition ? 1 : 0,
                p.Single_Vehicle ? 1 : 0,
                p.Distracted_Driving ? 1 : 0,
                p.Drowsy_Driving ? 1 : 0,
                p.Roadway_Departure ? 1 : 0
            };

            int[] dimensions = new int[] { 1, dataForModel.Length };

            DenseTensor<float> tensorForModel = new DenseTensor<float>(dataForModel, dimensions);

            var result = calculatorSession.Run(new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor<float>("float_input", tensorForModel)
            });

            Tensor<string> classification = result.First().AsTensor<string>();
            string predictedSeverity = classification.First();
            result.Dispose();

            ViewBag.predictedSeverity = predictedSeverity;

            return View(p);
        }

        public IActionResult Charts()
        {
            return View();
        }

    }
}
