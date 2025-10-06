using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Auth
{
    public class SendOtpDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
