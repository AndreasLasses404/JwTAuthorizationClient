using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Models.Responses
{
    class RefreshTokenResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
        public DateTime JwtExpires { get; set; }
    }
}
