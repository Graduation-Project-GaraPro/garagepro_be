using BusinessObject.RequestEmergency;
using Dtos.Emergency.Dtos.Emergency;
using Repositories.EmergencyRequestRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EmergencyRequestService
{
    public class PriceEmergenciesService : IPriceService
    {
        

       
            private readonly IPriceEmergencyRepositories _priceRepository;

            public PriceEmergenciesService(IPriceEmergencyRepositories priceRepository)
            {
                _priceRepository = priceRepository;
            }

            public async Task<PriceEmergency> GetCurrentPriceAsync()
            {
                return await _priceRepository.GetLatestPriceAsync();
            }

            // Tính phí emergency theo km
            public async Task<decimal> CalculateEmergencyFeeAsync(double distanceKm)
            {
                var price = await _priceRepository.GetLatestPriceAsync();

                if (price == null)
                    return 0;

                return price.BasePrice + (price.PricePerKm * (decimal)distanceKm);
            }

            public async Task AddPriceAsync(PriceEmergencyDto price)
            {
                await _priceRepository.AddPriceAsync(price);
                

            }
        }

    }
