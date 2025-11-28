using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Auth
{
    public class PasswordChangeRequest
    {
        public string PhoneNumber { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
}
