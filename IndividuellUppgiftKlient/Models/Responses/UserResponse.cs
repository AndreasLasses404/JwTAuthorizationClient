using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Models.Responses
{
    public class UserResponse
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
    }
}
