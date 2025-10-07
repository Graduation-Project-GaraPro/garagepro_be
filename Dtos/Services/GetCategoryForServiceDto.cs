using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class GetCategoryForServiceDto
    {
        public Guid ServiceCategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid ServiceTypeId { get; set; }
        public Guid? ParentServiceCategoryId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
