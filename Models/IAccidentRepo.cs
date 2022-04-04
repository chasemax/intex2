using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Models
{
    public interface IAccidentRepo
    {
        IQueryable<Accident> Accidents { get; }

        public void AddAccident(Accident a);
        public void UpdateAccident(Accident a);
        public void DeleteAccident(Accident a);
    }
}
