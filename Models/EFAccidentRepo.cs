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
