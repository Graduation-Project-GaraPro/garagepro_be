using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Services.GeocodingServices
{
    public class GoongGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoongGeocodingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Goong:ApiKey"]
                ?? throw new ArgumentNullException("Goong API key not found in configuration");
        }

        public async Task<(double lat, double lng, string formattedAddress)> GetCoordinatesAsync(string address)
        {
            var url = $"https://rsapi.goong.io/geocode?address={Uri.EscapeDataString(address)}&api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                throw new ApplicationException("No results found from Goong API.");

            var first = results[0];
            var geometry = first.GetProperty("geometry");
            var location = geometry.GetProperty("location");

            var lat = location.GetProperty("lat").GetDouble();
            var lng = location.GetProperty("lng").GetDouble();
            var formatted = first.GetProperty("formatted_address").GetString() ?? address;

            return (lat, lng, formatted);
        }
    }
}
