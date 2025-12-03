using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;
using Dtos.Branches;

namespace Services.BranchServices
{
    public interface IBranchService
    {
        Task<IEnumerable<BranchReadDto>> GetAllBranchesAsync();
        Task<IEnumerable<Branch>> GetAllBranchesBasicAsync();


        Task<(IEnumerable<BranchReadDto> Branches, int TotalCount)> GetAllBranchesAsync(int page, int pageSize, string? search, string? city, bool? isActive);
        Task<BranchReadDto?> GetBranchByIdAsync(Guid id);
        Task<BranchReadDto> CreateBranchAsync(BranchCreateDto dto);
        Task<BranchReadDto?> UpdateBranchAsync(BranchUpdateDto dto);
        Task UpdateIsActiveForManyAsync(IEnumerable<Guid> branchIds, bool isActive);
        Task DeleteManyAsync(IEnumerable<Guid> branchIds);
        Task<bool> DeleteBranchAsync(Guid id);
        Task<IEnumerable<object>> GetTechniciansWithWorkloadByBranchAsync(Guid branchId);
    }

}
