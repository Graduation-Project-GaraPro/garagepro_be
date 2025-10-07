using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Technician
{
    public class UpdateInspectionRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập kết quả kiểm tra")]
        public string Finding { get; set; }

        public bool IsCompleted { get; set; } = false;

        // Danh sách kết quả kiểm tra theo từng Service
        public List<ServiceInspectionUpdateDto> ServiceUpdates { get; set; } = new List<ServiceInspectionUpdateDto>();
    }

    public class ServiceInspectionUpdateDto
    {
        [Required]
        public Guid ServiceId { get; set; }

        // Trạng thái tình trạng dịch vụ (Tốt, Cần thay thế, Hư hỏng, v.v.)
        public ConditionStatus ConditionStatus { get; set; }

        // Gợi ý phụ tùng tương ứng với service này
        public List<Guid> SuggestedPartIds { get; set; } = new List<Guid>();
    }
}
