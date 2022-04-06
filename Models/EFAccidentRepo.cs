using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Models
{
    public class EFAccidentRepo : IAccidentRepo
    {
        private AccidentDbContext _adbc { get; set; }

        public IQueryable<Accident> Accidents => _adbc.Accidents;

        public EFAccidentRepo(AccidentDbContext temp) => _adbc = temp;

        public void AddAccident(Accident a)
        {
            int maxId = _adbc.Accidents.Select(x => x.Crash_ID).Max();
            a.Crash_ID = maxId + 1;
            a.City = a.City.ToUpper();
            a.County_Name = a.County_Name.ToUpper();
            a.Main_Road_Name = a.Main_Road_Name.ToUpper();
            _adbc.Add(a);
            _adbc.SaveChanges();
        }

        public void DeleteAccident(Accident a)
        {
            _adbc.Remove(a);
            _adbc.SaveChanges();
        }

        public void UpdateAccident(Accident a)
        {
            _adbc.Update(a);
            _adbc.SaveChanges();
        }
    }
}
