using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Models
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string Role { get; set; }

        public string JwtToken { get; set; }
        public DateTime JwtExpires { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
        public bool RefreshExpired => DateTime.Now >= RefreshExpires;
        public bool JwtExpired => DateTime.Now >= JwtExpires;



    }
}
