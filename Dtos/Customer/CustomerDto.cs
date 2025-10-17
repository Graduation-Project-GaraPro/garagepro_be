using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Customer
{
    public class CustomerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class CreateCustomerRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }
    }
}