using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestConsole;
using TrackingClient;


namespace TestClass
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();

            // Get environment variables
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables("")
                .Build();
            // You need to add these lines for accessing outside of Docker
            var url = config["ASPNETCORE_URLS"] ?? "http://*:8080";
            var env = config["ASPNETCORE_ENVIRONMENT"] ?? "Development";

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(url)
                .UseEnvironment(env)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        //.UseKestrel()
                        //.UseContentRoot(Directory.GetCurrentDirectory())
                        .UseUrls("http://*:5000")
                        //.UseIISIntegration()
                        .UseStartup<Startup>();
                });
    }
}
