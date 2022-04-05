using Intex2.Models;
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

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

        [HttpPost]
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
                        if (user.TwoFactorEnabled)
                        {
                            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                            var setupInfo = tfa.GenerateSetupCode("Authenticate", user.UserName, Encoding.ASCII.GetBytes(user.UserName));
                            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                            ViewBag.SetupCode = setupInfo.ManualEntryKey;
                            await _sim.SignOutAsync();

                            return View("Verify2FA", lm);
                        }
                        else
                        {
                            return Redirect(lm?.ReturnUrl ?? "/home/index");
                        }
                    }
                }
            }

            ModelState.AddModelError("", "Invalid Username or Password");
            return View(lm);
        }

        public async Task<IActionResult> Verify2FA()
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            string returnUrl = Request.Form["returnUrl"];
            if (returnUrl == null || returnUrl == "")
            {
                returnUrl = "/home/index";
            }
            IdentityUser user = await _um.FindByNameAsync(username);
            bool isValidLogin = (await _sim.PasswordSignInAsync(user, password, false, false)).Succeeded;

            if (user.TwoFactorEnabled)
            {
                var token = Request.Form["passcode"];
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                bool isValidTfaPasscode = tfa.ValidateTwoFactorPIN(user.UserName, token);
                if (isValidTfaPasscode && isValidLogin)
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    await _sim.SignOutAsync();
                    ModelState.AddModelError("", "Invalid MFA token");
                    LoginModel lm = new LoginModel();
                    lm.Username = username;
                    lm.Password = password;
                    lm.ReturnUrl = returnUrl;
                    return View(lm);
                }
            }
            else
            {
                return Redirect(returnUrl);
            }
        }

        public async Task<RedirectResult> Logout (string returnUrl="/")
        {
            await _sim.SignOutAsync();

            return Redirect(returnUrl);
        }
    }
}
