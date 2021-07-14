using ClassScraper.Repository.EntityService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScraperApi.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScraperApi
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
            services.AddControllers(options =>
            {
                options.Filters.Add(new ExceptionFilter());
            }).AddNewtonsoftJson();

            services.AddEntityService(Configuration, Log.Logger);
            services.CheckIndexes(Configuration, Log.Logger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();

            app.Use(async (context, next) =>
            {
                var section = Configuration["Users"];
                if (section == null)
                {
                    await next.Invoke();
                    return;
                }

                var users = new Dictionary<string, string>();
                foreach (var str in Configuration["Users"].Split(";"))
                {
                    var temp = str.Split('=');
                    users.Add(temp[0], temp[1]);
                }

                if (context.Request.Headers.TryGetValue("Authorization", out var values))
                {
                    var authString = Encoding.UTF8.GetString(Convert.FromBase64String(values.First().Substring(6)));
                    var authUser = authString.Split(":");

                    if (users.TryGetValue(authUser[0], out string pass) && pass == authUser[1])
                    {
                        await next.Invoke();
                        return;
                    }
                }

                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Wrong Authorization Header");
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
