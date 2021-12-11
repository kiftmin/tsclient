using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackingClient.Models
{
    public class TagTest
    {
        [Key]
        public string TagID { get; set; }
        public string TagNumber { get; set; }
        public string ClientName { get; set; }
        public string Reader { get; set; }
        public bool ReaderConn { get; set; }
        public DateTime Time { get; set; }
        //public ICollection<Enrollment> Enrollments { get; set; }
      
    }
}