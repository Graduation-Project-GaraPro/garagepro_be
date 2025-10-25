using Dtos.Statistical;
using System.Threading.Tasks;

namespace Services.Statistical
{
    public interface IStatisticalService
    {
        Task<TechnicianStatisticDto> GetTechnicianStatisticAsync(string userId);
    }
}
