using System;
using System.Collections.Generic;

namespace Intex2.Models.ViewModels
{
    public class PageInfo
    {
        public int TotalNumCrashes { get; set; }

        public int CrashesPerPage { get; set; }

        public int CurrentPage { get; set; }

        //Figure out how many pages we need
        public int TotalPages => (int)Math.Ceiling((double)TotalNumCrashes / CrashesPerPage);

        public List<KeyValuePair<string, Dictionary<string, string>>> CityFilterQueries { get; set; }
        public List<KeyValuePair<string, Dictionary<string, string>>> CountyFilterQueries { get; set; }
        public List<KeyValuePair<string, Dictionary<string, string>>> SeverityFilterQueries { get; set; }

        public string SelectedSeverity { get; set; }
        public string SelectedCity { get; set; }
        public string SelectedCounty { get; set; }
    }
}
