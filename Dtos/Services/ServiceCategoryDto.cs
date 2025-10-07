using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Branches;

namespace Dtos.Services
{
    public class ServiceCategoryDto
    {
        public Guid ServiceCategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid ServiceTypeId { get; set; }
        public Guid? ParentServiceCategoryId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Danh sách dịch vụ thuộc category này
        public List<ServiceDto> Services { get; set; }

        // Danh sách category con (tạo cấu trúc cây)
        public List<ServiceCategoryDto> ChildCategories { get; set; }
    }
}
