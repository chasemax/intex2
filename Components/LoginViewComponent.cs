using Intex2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Components
{
    public class LoginViewComponent : ViewComponent
    {
        private IAccidentRepo repo { get; set; }
        private UserManager<IdentityUser> _um;

        public LoginViewComponent(IAccidentRepo temp, UserManager<IdentityUser> temp2) {
            repo = temp;
            _um = temp2;
        }

        public IViewComponentResult Invoke()
        {
            LoginBarInfo lbi = new LoginBarInfo();
            lbi.IsLoggedIn = HttpContext.User.Identity.IsAuthenticated;
            lbi.Username = HttpContext.User.Identity.Name;
            return View(lbi);
        }
    }
}
