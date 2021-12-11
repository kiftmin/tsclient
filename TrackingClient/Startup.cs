using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingClient.Models;
using Microsoft.EntityFrameworkCore;
using TestConsole;
using TrackingClient.Services;
using TestClass;
using BranSystems.MQTT.Device.RFIDReader;
using RFIDReader;
using MQTTnet.Client.Options;
using System.Security.Cryptography.X509Certificates;
using IOClasses;
using BranSystems.MQTT.Device.IOController;
using System.Data.SqlClient;
using Newtonsoft.Json.Serialization;
using TrackingClient.Hubs;

namespace TrackingClient
{
    public class Startup
    {
        //public static string ReaderTag { get; set; }
        //public static bool ReaderConnectionStatus { get; set; } = false;
        //public static bool IOConnectionStatus { get; set; } = false;
        //public static DateTime ReaderConnectionStatusDate { get; set; } = DateTime.Now;
        //public static RFIDReaderTest MQTTReader { get; private set; }
        //public static IOTest MQTT_IO { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MyAppSettings>(Configuration.GetSection(MyAppSettings.SectionName));

            services.AddSignalR();

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            services.AddControllersWithViews();
            //TODO - add back
            //services.AddControllers().AddNewtonsoftJson(options =>
            //{
            //    // Use the default property (Pascal) casing
            //    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //});

            services.AddDbContext<DBCtx>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("TrackingClientDatabase")));

            Global.MQTTReader =  StartReader(Configuration.GetSection(MyAppSettings.SectionName).GetValue<string>("Reader").Split(" "));
            Global.MQTT_IO = StartIO(Configuration.GetSection(MyAppSettings.SectionName).GetValue<string>("IO").Split(" "));
            //HandleTagRecievedEvent = HandleTagEvent
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<PageHub>("/pagehub");
            });

            //Global.TestProcessingState();
        }


        public RFIDReaderTest StartReader(string[] args)
        {
            List<string> lst = new List<string>(args);
            System.Diagnostics.Debug.WriteLine("Starting test client for RFID Reader");
            return new RFIDReaderTest(lst.ToArray());
        }

        public IOTest StartIO(string[] args)
        {
            List<string> lst = new List<string>(args);
            System.Diagnostics.Debug.WriteLine("Starting test client for IO");
            return new IOTest(lst.ToArray());
        }
    }
}

