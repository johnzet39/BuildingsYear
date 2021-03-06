using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingsYear.Infrastructure;
using BuildingsYear.Models;
using BuildingsYear.Models.JSONModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingsYear
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
            string connectionString = Configuration.GetConnectionString("CONNECTION_BASE");
            string connectionStringGDAL = Configuration.GetConnectionString("CONNECTION_BASE_GDAL");
            string connectionBaseProvider = Configuration.GetSection("BaseProvider").Value;
            string mbtilesstring = Configuration.GetSection("MbTiles").Value;

            services.Configure<List<JsonLayer>>(Configuration.GetSection("LayersList:JsonLayer"));
            services.Configure<ConnectionOptions>(options =>
            {
                options.ConnString = connectionString;
                options.ConnStringGDAL = connectionStringGDAL;
                options.BaseProvider = connectionBaseProvider;
            });

            services.AddSingleton(new MbTilesReader(mbtilesstring));
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation(); //update page after refresh (install nuget Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddFile("Logs/log.log", minimumLevel: LogLevel.Warning);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
