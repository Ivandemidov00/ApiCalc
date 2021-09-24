using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCalc.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ApiCalc.Middleware;

namespace ApiCalc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IService,ServiceCalc>();
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Webtest", Version = "v1"}); });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Webtest v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseMaxConcurrentRequests() .Run(async (context) =>
            {
                await Task.Delay(500);

                await context.Response.WriteAsync("-- Demo.AspNetCore.MaxConcurrentConnections --");
            });
        }
    }
}