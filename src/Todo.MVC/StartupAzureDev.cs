﻿using Custom.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Todo.MVC.Services;
using ToDo.Core.EF;
using ToDo.Core.Models;
using ToDo.Core.Repos;
using ToDo.Core.Service;

namespace Todo.MVC
{
    public class StartupAzureDev
    {
        public IConfigurationRoot Configuration { get; }
        public StartupAzureDev(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            //You add a middleware 
            services.AddMvc();
            //Add Entity Framework here
            //HACK: Current tooling Preview-1 does not support EF migration target a class library
            // hence specifying the migration assembly as web app and that is why we end up having migrations
            // added to the web app instead of the class library that has the context
            services.AddEntityFramework()
                .AddDbContext<ToDoDataContext>(opts => opts.UseInMemoryDatabase());
            //Add the identity provider
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ToDoDataContext>()
                    .AddDefaultTokenProviders();


            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IToDoRepository, InMemoryToDoRepository>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            // Need to hook this up before the other middleware is hooked up
            // this does not use the extension method
            // app.UseMiddleware<HeaderMiddleware>(new HeaderOptions {HeaderName="X-Powered-By", HeaderValue="ASPNET_CORE" });
            //using the extension method
            app.UseCustomHeader(new HeaderOptions { HeaderName = "X-TODO-Powered-By", HeaderValue = "ASPNET_CORE" });

            // without static files not event html or images will be served up
            app.UseStaticFiles();

            // Add another custom middle ware that adds the execution time as a header 
            app.UsePipelineTimer();
            // Via diagnostics.. shows the new yellow screen of death

            // Need to add .adddebug and  console logging level should watch if you want debug messages to popup
            loggerFactory.AddConsole(LogLevel.Debug);
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
           
            //Note order is important use identity has to come before you call 
            // external login  provider else it won't work
            app.UseIdentity();
            app.UseTODOMVCOAuthAuthentication(Configuration);
     
            app.UseMvc(routes =>
             routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}/{type?}"
                 ));




        }
    }
}
