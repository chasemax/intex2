using System;
using System.Linq;
namespace Intex2.Models.ViewModels
{
    public class AccidentsViewModel
    {
        public IQueryable<Accident> Accidents { get; set; }

        public PageInfo PageInfo { get; set; }
    }
}
