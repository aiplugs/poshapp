using Aiplugs.PoshApp.Formatters;
using Aiplugs.PoshApp.Services;
using AutoMapper;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aiplugs.PoshApp
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
            services.AddMemoryCache();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton<ConfigAccessor>();
            services.AddSingleton<ScriptsService>();
            services.AddSingleton<PowershellContext>();
            services.AddSingleton<GitContext>();

            services.AddControllersWithViews(options => {
                options.InputFormatters.Add(new PlainTextInputFormatter());
            })
            .AddRazorRuntimeCompilation()
            .AddNewtonsoftJson();
            services.AddSignalR();
            services.AddHostedService<PowershellWorker>();
            services.AddHostedService<GitWorker>();
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
                app.UseExceptionHandler("/Error");
            }

            if (HybridSupport.IsElectronActive)
            {
                ElectronBootstrap();
            }
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Default}/{action=Index}/{id?}");
                endpoints.MapHub<PoshAppHub>("/poshapp");
            });
        }
        public async void ElectronBootstrap()
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 1152,
                Height = 864,
                Show = false,
                WebPreferences = new WebPreferences
                {
                    NodeIntegration = false
                }
            });

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle("Electron.NET API Demos");
        }
    }
}
