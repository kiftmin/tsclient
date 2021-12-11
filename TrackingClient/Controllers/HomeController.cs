using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TrackingClient.Models;
using Microsoft.Extensions.Options;
using TrackingClient.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using TrackingClient.Hubs;

namespace TrackingClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<MyAppSettings> _options;
        private readonly DBCtx _context;

        private DBCtx Context { get; }
        public HomeController(ILogger<HomeController> logger, DBCtx _context, IOptions<MyAppSettings> options, DBCtx context, IHubContext<PageHub> hubContext)
        {
            this.Context = _context;
            _logger = logger;
            _options = options;
            _context = context;
            Global.HubContext = hubContext;
        }

        public IActionResult Index()
        {
            ViewBag.ReaderOptions = _options.Value.Reader;
            ViewBag.IOOptions = _options.Value.IO;

            return View();
        }

        public IActionResult Test()
        {

            return View();
        }

        public IActionResult SaveTag()
        {
            Global.MQTTReader.RequestReadTag();

            //
            return RedirectToAction("Index");

        }


        public IActionResult ReadTag()
        {
            
          Global.MQTTReader.RequestReadTag();
           return RedirectToAction("Index");

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
