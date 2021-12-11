using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackingClient.Services
{
    public class MyAppSettings
    {
        public const string SectionName = "MySettings";
  
        public string ABC { get; set; }
        public string DEF { get; set; }
        public int XYZ { get; set; }
        public string Reader { get; set; }
        public string IO { get; set; }
        public string ClientName { get; set; }
    }
}
