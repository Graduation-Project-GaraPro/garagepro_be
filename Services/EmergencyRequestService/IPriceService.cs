using BusinessObject.RequestEmergency;
using Dtos.Emergency.Dtos.Emergency;

namespace Services.EmergencyRequestService
{
    public interface IPriceService
    {
            Task<PriceEmergency> GetCurrentPriceAsync();
            Task<decimal> CalculateEmergencyFeeAsync(double distanceKm);
            Task AddPriceAsync(PriceEmergencyDto price);
        }
    }
