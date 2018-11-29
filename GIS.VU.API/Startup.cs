using System;
using System.Diagnostics;
using LoadGIS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchGIS;

namespace GIS.VU.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc();

            var watch = Stopwatch.StartNew();
            // the code that you want to measure comes here

            services.AddSingleton(new SearchEngine(new Loader(".\\Data\\grid.txt", ".\\Data\\")));

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine(elapsedMs);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseCors(builder =>
                builder.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyMethod());

            app.UseMvc();
        }
    }
}