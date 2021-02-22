using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient.Models.Requests
{
    class UpdateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Role { get; set; }
    }
}
