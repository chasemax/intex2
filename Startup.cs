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
using System.Configuration;

namespace Intex2
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            CurrentEnvironment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<AccidentDbContext>(options =>
            {
                if (CurrentEnvironment.IsDevelopment())
                {
                    options.UseMySql(Configuration["ConnectionStrings:AccidentDBConnectionDev"]);
                }
                else
                {
                    //ConfigurationManager.ConnectionStrings["IdentityDBConnectionProd"].ConnectionString;
                    options.UseMySql(Configuration["ConnectionStrings:AccidentDBConnectionProd"]);
                }
            });

            services.AddScoped<IAccidentRepo, EFAccidentRepo>();

            services.AddRazorPages();
            services.AddDistributedMemoryCache();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                if (CurrentEnvironment.IsDevelopment())
                {
                    options.UseMySql(Configuration["ConnectionStrings:IdentityDbConnectionDev"]);
                }
                else
                {
                    options.UseMySql(Configuration["ConnectionStrings:IdentityDbConnectionProd"]);
                }
            });
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6; // Change this before deployment
                options.Password.RequiredUniqueChars = 1;
            });
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

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';img-src 'self' data: https://maps.googleapis.com;style-src 'self' 'unsafe-hashes' 'sha256-aqNNdDLnnrDOnTNdkJpYlAxKVJtLt9CtFLklmInuUAE='");
                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/admin/{*catchall}", "/admin/index");
            });

            IdentitySeedData.EnsurePopulated(app);
        }
    }
}
