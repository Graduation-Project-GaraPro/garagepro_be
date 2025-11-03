using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class UpdateServiceCategoryDto
    {
        [Required]
        [MaxLength(100),MinLength(3)]
        public string CategoryName { get; set; }


        public Guid? ParentServiceCategoryId { get; set; }

        [MaxLength(500),MinLength(10)]
        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
