using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using Dtos.InspectionAndRepair;
using Repositories.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.InspectionAndRepair
{
    public class JobTechnicianService : IJobTechnicianService
    {
        private readonly IJobTechnicianRepository _jobTechnicianRepository;
        private readonly IMapper _mapper;

        public JobTechnicianService(IJobTechnicianRepository jobTechnicianRepository, IMapper mapper)
        {
            _jobTechnicianRepository = jobTechnicianRepository;
            _mapper = mapper;
        }

        public async Task<List<JobTechnicianDto>> GetJobsByTechnicianAsync(string userId)
        {

            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);

            // Lọc các trạng thái cần thiết
            var filteredJobs = jobs
                .Where(j => j.Status == JobStatus.New ||
                            j.Status == JobStatus.InProgress ||
                            j.Status == JobStatus.OnHold ||
                            j.Status == JobStatus.Completed)
                .ToList();

            return _mapper.Map<List<JobTechnicianDto>>(filteredJobs);
        }

        public async Task<JobTechnicianDto?> GetJobByIdAsync(string userId, Guid jobId)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);
            var job = jobs.FirstOrDefault(j => j.JobId == jobId);
            return job == null ? null : _mapper.Map<JobTechnicianDto>(job);
        }
        public async Task<bool> UpdateJobStatusAsync(string userId, JobStatusUpdateDto dto)
        {
            var jobs = await _jobTechnicianRepository.GetJobsByTechnicianAsync(userId);
            var job = jobs.FirstOrDefault(j => j.JobId == dto.JobId);

            //Kiểm tra Job tồn tại và thuộc quyền Technician
            if (job == null)
                throw new Exception("Công việc không tồn tại hoặc bạn không có quyền cập nhật công việc này.");

            //Kiểm tra Job có Repair chưa
            if (job.Repair == null)
                throw new Exception("Không thể cập nhật trạng thái vì công việc này chưa có thông tin sửa chữa (Repair).");

            //Kiểm tra trạng thái hợp lệ (chỉ 2, 3, 4)
            var validStatuses = new[] { JobStatus.InProgress, JobStatus.Completed, JobStatus.OnHold };
            if (!validStatuses.Contains(dto.JobStatus))
                throw new Exception("Chỉ được cập nhật trạng thái giữa: InProgress, Completed, và OnHold.");

            //Nếu job đã Completed rồi thì không được đổi trạng thái nữa
            if (job.Status == JobStatus.Completed && dto.JobStatus != JobStatus.Completed)
                throw new Exception("Không thể thay đổi trạng thái vì công việc này đã hoàn thành.");

            //Nếu trạng thái hiện tại không thuộc nhóm cho phép
            if (!validStatuses.Contains(job.Status))
                throw new Exception("Không thể cập nhật công việc vì trạng thái hiện tại không hợp lệ để chuyển đổi.");

            //Cập nhật trạng thái
            job.Status = dto.JobStatus;
            job.UpdatedAt = DateTime.UtcNow;

            //Tính thời gian bắt đầu và kết thúc sửa chữa

            if (dto.JobStatus == JobStatus.Completed)
            {
                var repair = job.Repair;
                repair.EndTime = DateTime.UtcNow;

                if (repair.StartTime.HasValue)
                {
                    repair.ActualTime = repair.EndTime.Value - repair.StartTime.Value;
                }
            }
            //if (repair.StartTime.HasValue)
            //{
            //    var duration = repair.EndTime.Value - repair.StartTime.Value;
            //    if (duration.TotalHours >= 24)
            //        duration = TimeSpan.FromHours(duration.TotalHours % 24);
            //    repair.ActualTime = duration;
            //}

            await _jobTechnicianRepository.UpdateJobAsync(job);
            return true;
        }




    }
}
