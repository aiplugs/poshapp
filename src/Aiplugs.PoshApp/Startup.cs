using Aiplugs.PoshApp.Formatters;
using Aiplugs.PoshApp.Services;
using Aiplugs.PoshApp.Services.Git;
using Aiplugs.PoshApp.Services.Powersehll;
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
            services.AddSingleton<LicenseService>();
            services.AddSingleton<ScriptsService>();
            services.AddSingleton<PowershellContext>();
            services.AddSingleton<GitContext>();

            services.AddControllersWithViews(options => {
                options.InputFormatters.Add(new PlainTextInputFormatter());
            })
            .AddRazorRuntimeCompilation()
            .AddNewtonsoftJson();
            services.AddSignalR();
            services.AddHttpClient();
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
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Default}/{action=Index}/{id?}");
                endpoints.MapHub<PoshAppHub>("/poshapp");
            });

            if (HybridSupport.IsElectronActive)
            {
                ElectronBootstrap();
                ElectronIpc.Setup();
            }
        }
        public async void ElectronBootstrap()
        {
            await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Title = "POSH App",
                Width = 1152,
                Height = 864,
                AutoHideMenuBar = true,
                Show = true,
                Icon = "~/icon/poshapp-icon-256x256.png"
            });
        }
    }
}
