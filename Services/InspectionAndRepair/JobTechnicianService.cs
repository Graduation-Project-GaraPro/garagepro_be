using AutoMapper;
using BusinessObject;
using BusinessObject.Enums;
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

            if (job == null)
                throw new Exception("Bạn không có quyền cập nhật công việc này hoặc Job không tồn tại.");

            //// Kiểm tra trạng thái hiện tại
            //if (job.Status == JobStatus.Completed && dto.JobStatus != JobStatus.Completed)
            //    throw new Exception("Không thể thay đổi trạng thái của Job đã hoàn thành.");

            job.Status = dto.JobStatus;
            job.UpdatedAt = DateTime.UtcNow;

            await _jobTechnicianRepository.UpdateJobAsync(job);
            return true;
        }


    }
}
