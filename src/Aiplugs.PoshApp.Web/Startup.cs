using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Aiplugs.PoshApp.Deamon;
using Aiplugs.PoshApp.Web.Models;
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var poshappDir = Configuration["POSHAPP_DIR"]
                    ?? Path.Combine(Environment.GetEnvironmentVariable("WEBAPP_STORAGE_HOME")
                        ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".poshapp");

                endpoints.MapGet("/powershell", async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
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

                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = new WebSocketMessageHandler(socket);
                    var service = new GitService(handler, credentialManager, poshappDir);

                    await service.StartAsync();
                });

                endpoints.MapGet("/pses", async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }
                    var ext = Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : string.Empty;
                    var bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"pses/bin/Common/Aiplugs.PoshApp.Pses" + ext);
                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = bin,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    });

                    var connector = new WebSocketStreamConnecter(socket, process.StandardOutput, process.StandardInput);

                    await connector.StartAsync();

                    await process.StandardInput.DisposeAsync();
                    process.StandardOutput.Dispose();
                    process.Kill();
                });
            });
        }
    }
}
