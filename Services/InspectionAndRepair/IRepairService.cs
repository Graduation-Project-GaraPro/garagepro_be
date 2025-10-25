using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public interface IRepairService
    {
        Task<RepairDetailDto> GetRepairOrderDetailsAsync(Guid repairOrderId, Guid technicianId);
        Task<RepairResponseDto> CreateRepairAsync(Guid technicianId, RepairCreateDto dto);
        Task<Repair> UpdateRepairAsync(Guid technicianId, Guid repairId, RepairUpdateDto dto);
    }
}
