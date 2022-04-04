using Intex2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<IdentityUser> _um;
        private SignInManager<IdentityUser> _sim;

        public AccountController(UserManager<IdentityUser> t1, SignInManager<IdentityUser> t2)
        {
            _um = t1;
            _sim = t2;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            LoginModel lm = new LoginModel();
            lm.ReturnUrl = returnUrl;
            return View(lm);
        }

        public async Task<IActionResult> Login (LoginModel lm)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _um.FindByNameAsync(lm.Username);

                if (user != null)
                {
                    await _sim.SignOutAsync();

                    if ((await _sim.PasswordSignInAsync(user, lm.Password, false, false)).Succeeded)
                    {
                        return Redirect(lm?.ReturnUrl ?? "/admin");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid Username or Password");
            return View(lm);
        }

        public async Task<RedirectResult> Logout (string returnUrl="/")
        {
            await _sim.SignOutAsync();

            return Redirect(returnUrl);
        }
    }
}
