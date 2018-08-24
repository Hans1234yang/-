using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class studnet
    {
        [Key]
        public int id { get; set; }

        public string stuname { get; set; }

        public int hobbyid { get; set; }

        public int cityid { get; set; }
    }
}