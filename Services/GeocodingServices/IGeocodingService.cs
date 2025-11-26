using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeocodingServices
{
    public interface IGeocodingService
    {
        Task<(double lat, double lng, string formattedAddress)> GetCoordinatesAsync(string address);
        Task<string> ReverseGeocodeAsync(double lat, double lng);
    }
}
