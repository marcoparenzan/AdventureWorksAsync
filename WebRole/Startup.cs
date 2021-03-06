﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebRole.Services;

namespace WebRole
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
            services.AddMvc();
            services.AddSingleton<IHostedService, CommandResponsesService>();
            services.AddSingleton<ISignalRRegistry, SignalRRegistry>();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "spa",
                    template: "/pippo/{*tuttoilrestoeilnulla}", defaults: new {
                        controller = "spa",
                        action = "page"
                    });

                routes.MapRoute(
                    name: "default",
                    template: "/pages/{controller=Home}/{action=Index}");
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<CommandResponsesHub>("/commandresponses");
            });
        }
    }
}
