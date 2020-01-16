using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceAPI.Controllers;
using FaceAPI.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FaceAPI
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        private readonly IWebHostEnvironment hostingEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<StorageOptions>(x =>
            {
                x.FullImageContainerName = configuration["Storage:FullImageContainerName"];
                x.StorageConnectionString = configuration["Storage:StorageConnectionString"];
                x.ThumbnailImageContainerName = configuration["Storage:ThumbnailImageContainerName"];
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapFallback(async context =>
                {
                    await context.Response.WriteAsync("fallback endpoint");
                });
            });
        }
    }
}
