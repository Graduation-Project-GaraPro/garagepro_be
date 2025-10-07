using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Auth
{
    public class UpdateUserDto
    {
        public bool Gender { get; set; } // true = Male, false = Female

        // Legacy properties
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string? Avatar { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
