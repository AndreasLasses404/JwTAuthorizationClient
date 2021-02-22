using System;
using System.Collections.Generic;
using System.Text;

namespace IndividuellUppgiftKlient
{
    class RegisterUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int EmpId { get; set; }
    }
}
