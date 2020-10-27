using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aiplugs.PoshApp.Deamon;
using Aiplugs.PoshApp.Services;
using Aiplugs.PoshApp.Web.Models;
using LibGit2Sharp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StreamJsonRpc;

namespace Aiplugs.PoshApp.Web
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
            services.AddControllers();
            services.AddSingleton<ICredentialManager>(new MemoryCredentialManager());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ICredentialManager credentialManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var poshappDir = Configuration["PoshappDir"]
                    ?? Path.Combine(Environment.GetEnvironmentVariable("HOME")
                        ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".poshapp");

                endpoints.MapGet("/powershell", async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = new WebSocketMessageHandler(socket);
                    var service = new PoshAppService(handler, poshappDir);

                    await service.StartAsync();
                });

                endpoints.MapGet("/git", async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = new WebSocketMessageHandler(socket);
                    var service = new GitService(handler, credentialManager, poshappDir);

                    await service.StartAsync();
                });
            });
        }
    }
}
