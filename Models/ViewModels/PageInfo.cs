using System;

namespace Intex2.Models.ViewModels
{
    public class PageInfo
    {
        public int TotalNumCrashes { get; set; }

        public int CrashesPerPage { get; set; }

        public int CurrentPage { get; set; }

        //Figure out how many pages we need
        public int TotalPages => (int)Math.Ceiling((double)TotalNumCrashes / CrashesPerPage);
    }
}
