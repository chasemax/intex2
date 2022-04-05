using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Intex2.Models;
using Microsoft.EntityFrameworkCore;

namespace Intex2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<AccidentDbContext>(options =>
            {
                options.UseMySql(Configuration["ConnectionStrings:AccidentDBConnection"]);
            });

            services.AddScoped<IAccidentRepo, EFAccidentRepo>();

            services.AddRazorPages();
            services.AddDistributedMemoryCache();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseMySql(Configuration["ConnectionStrings:IdentityDbConnection"]));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "city",
                    pattern: "City{city}/County{county}",
                    defaults: new { Controller = "Home", Action = "Search" });

                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/admin/{*catchall}", "/admin/index");
            });

            IdentitySeedData.EnsurePopulated(app);
        }
    }
}
