using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "admin";
        private const string adminPassword = "Administrator123!";
        private const string mfaUser = "mfaadmin";
        private const string mfaPassword = "Multifactor123!";
        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            AppIdentityDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<AppIdentityDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            UserManager<IdentityUser> userManager =
                app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            IdentityUser user = await userManager.FindByIdAsync(adminUser);
            if (user == null)
            {
                user = new IdentityUser(adminUser);
                user.Email = "admin@example.com";
                user.PhoneNumber = "555-1234";
                await userManager.CreateAsync(user, adminPassword);
            }

            IdentityUser user2 = await userManager.FindByIdAsync(mfaUser);
            if (user2 == null)
            {
                user2 = new IdentityUser(mfaUser);
                user2.Email = "mfaadmin@example.com";
                user2.PhoneNumber = "333-1234";
                user2.TwoFactorEnabled = true;
                await userManager.CreateAsync(user2, mfaPassword);
            }

        }
    }
}
