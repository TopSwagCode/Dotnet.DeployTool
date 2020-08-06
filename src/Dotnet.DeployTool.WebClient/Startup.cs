using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.DeployTool.Core;
using Dotnet.DeployTool.WebClient.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dotnet.DeployTool.WebClient
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
            services.AddRazorPages();

            services.AddSignalR();

            string ip = "54.216.46.117";
            services.AddSingleton(new DeployServiceConfiguration(new Renci.SshNet.PrivateKeyFile("C:\\git\\Dotnet.DeployTool\\src\\Dotnet.DeployTool.CLI\\default.pem"), ip, 22, "")); // should be moved to client to specify this

            services.AddSingleton<IScriptManager, ScriptManager>();

            services.AddTransient<IFeedbackChannel, SignalRFeedbackChannel>();
            services.AddTransient<IDeployService, DeployService>();

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chathub");
                endpoints.MapHub<DeployHub>("/deployhub");
            });
        }
    }
}
