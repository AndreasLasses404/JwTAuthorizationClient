using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
        public bool RefreshIsExpired => DateTime.Now >= RefreshExpires;
        public DateTime JwtExpires { get; set; }
        public bool JwtIsExpired => DateTime.Now >= JwtExpires;
    }
}
