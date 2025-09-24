using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Auth
{
    public class ForcePasswordChangeRequest
    {
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
