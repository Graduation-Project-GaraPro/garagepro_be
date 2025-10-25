using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using Repositories.InspectionAndRepair;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public class RepairService : IRepairService
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IMapper _mapper;

        public RepairService(IRepairRepository repairRepository, IMapper mapper)
        {
            _repairRepository = repairRepository;
            _mapper = mapper;
        }

        public async Task<RepairDetailDto> GetRepairOrderDetailsAsync(Guid repairOrderId, Guid technicianId)
        {
            var repairOrder = await _repairRepository.GetRepairOrderWithJobsAsync(repairOrderId);
            if (repairOrder == null)
                throw new Exception("Không tìm thấy Repair Order.");

            // Kiểm tra xem technician có được phân công job nào trong Repair Order này không
            bool isAssigned = repairOrder.Jobs
                .Any(j => j.JobTechnicians.Any(t => t.TechnicianId == technicianId));

            if (!isAssigned)
                throw new Exception("Bạn không được phân công bất kỳ Job nào trong Repair Order này.");

            
            return _mapper.Map<RepairDetailDto>(repairOrder);
        }


        public async Task<RepairResponseDto> CreateRepairAsync(Guid technicianId, RepairCreateDto dto)
        {
            var job = await _repairRepository.GetJobByIdAsync(dto.JobId);
            if (job == null)
                throw new Exception("Không tìm thấy Job.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, dto.JobId);
            if (!isAssigned)
                throw new Exception("Technician không có quyền thực hiện Job này.");

            if (job.Status == JobStatus.Completed)
                throw new Exception("Job đã hoàn thành, không thể tạo Repair mới.");


            if (job.Repair != null)
                throw new Exception("Job này đã có Repair, không thể tạo thêm.");


            if (string.IsNullOrWhiteSpace(dto.EstimatedTime))
                throw new Exception("EstimatedTime không được để trống.");

            if (!TimeSpan.TryParse(dto.EstimatedTime, out var estimatedTime) || estimatedTime <= TimeSpan.Zero)
                throw new Exception("Định dạng EstimatedTime không hợp lệ. Vui lòng nhập theo dạng HH:mm:ss (ví dụ: 02:30:00).");


            var repair = _mapper.Map<Repair>(dto);
            repair.StartTime = DateTime.UtcNow;

            await _repairRepository.AddRepairAsync(repair);
            job.Status = JobStatus.InProgress;
            await _repairRepository.SaveChangesAsync();

            return _mapper.Map<RepairResponseDto>(repair);
        }



        public async Task<Repair> UpdateRepairAsync(Guid technicianId, Guid repairId, RepairUpdateDto dto)
        {
            var repair = await _repairRepository.GetRepairByIdAsync(repairId);
            if (repair == null)
                throw new Exception("Không tìm thấy Repair.");

            var job = await _repairRepository.GetJobByIdAsync(repair.JobId);
            if (job == null)
                throw new Exception("Không tìm thấy Job liên quan.");

            bool isAssigned = await _repairRepository.TechnicianHasJobAsync(technicianId, job.JobId);
            if (!isAssigned)
                throw new Exception("Technician không có quyền cập nhật Job này.");

            if (job.Status != JobStatus.InProgress && job.Status != JobStatus.OnHold)
                throw new Exception("Chỉ được cập nhật khi Job đang InProgress hoặc OnHold.");

            repair.Description = dto.Description;
            repair.Notes = dto.Notes;

            await _repairRepository.UpdateRepairAsync(repair);
            await _repairRepository.SaveChangesAsync();

            return repair;
        }
    }
}
