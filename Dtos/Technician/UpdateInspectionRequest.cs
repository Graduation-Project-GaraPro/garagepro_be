using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Technician
{
    public class UpdateInspectionRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập kết quả kiểm tra")]
        public string Finding { get; set; }
        public List<Guid> SuggestedPartIds { get; set; } = new List<Guid>();

        public bool IsCompleted { get; set; } = false;
    }
}
