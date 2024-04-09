using System.Net.Http;
using System.Text.Json;
using Azure.Core;
using TrybeHotel.Dto;
using TrybeHotel.Repository;

namespace TrybeHotel.Services
{
    public class GeoService : IGeoService
    {
        private readonly HttpClient _client;
        public GeoService(HttpClient client)
        {
            _client = client;
        }

        // 11. Desenvolva o endpoint GET /geo/status
        public async Task<object> GetGeoStatus()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://nominatim.openstreetmap.org/status.php?format=json");

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "TrybeHotel");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }

            return null;

        }

        // 12. Desenvolva o endpoint GET /geo/address
        public async Task<GeoDtoResponse> GetGeoLocation(GeoDto geoDto)
        {
            var url = $"https://nominatim.openstreetmap.org/search?street={geoDto.Address}&city={geoDto.City}&country=Brazil&state={geoDto.State}&format=json&limit=1";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("User-Agent", "aspnet-user-agent");

            var response = await _client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                return default(GeoDtoResponse);
            }

            var result = await response.Content.ReadFromJsonAsync<GeoDtoResponse[]>();
            return result?.FirstOrDefault();
        }

        // 12. Desenvolva o endpoint GET /geo/address
        public async Task<List<GeoDtoHotelResponse>> GetHotelsByGeo(GeoDto geoDto, IHotelRepository repository)
        {
            var newDistance = await GetGeoLocation(geoDto);

            if (newDistance == null)
            {
                return new List<GeoDtoHotelResponse>();
            }

            var Hotels = repository.GetHotels();

            var hotelsWithDistances = await Task.WhenAll(Hotels.Select(async hotel =>
            {
                var hotelCoordinates = await GetGeoLocation(new GeoDto
                {
                    Address = hotel.address,
                    City = hotel.cityName,
                    State = hotel.state
                });

                if (hotelCoordinates != null)
                {
                    int distance = CalculateDistance(
                        newDistance.lat,
                        newDistance.lon,
                        hotelCoordinates.lat,
                        hotelCoordinates.lon
                    );

                    return new GeoDtoHotelResponse
                    {
                        HotelId = hotel.hotelId,
                        Name = hotel.name,
                        Address = hotel.address,
                        CityName = hotel.cityName,
                        State = hotel.state,
                        Distance = distance
                    };
                }

                return null;
            }));

            return hotelsWithDistances
                .Where(h => h != null)
                .OrderBy(h => h.Distance)
                .ToList();


        }


        public int CalculateDistance(string latitudeOrigin, string longitudeOrigin, string latitudeDestiny, string longitudeDestiny)
        {
            double latOrigin = double.Parse(latitudeOrigin.Replace('.', ','));
            double lonOrigin = double.Parse(longitudeOrigin.Replace('.', ','));
            double latDestiny = double.Parse(latitudeDestiny.Replace('.', ','));
            double lonDestiny = double.Parse(longitudeDestiny.Replace('.', ','));
            double R = 6371;
            double dLat = radiano(latDestiny - latOrigin);
            double dLon = radiano(lonDestiny - lonOrigin);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(radiano(latOrigin)) * Math.Cos(radiano(latDestiny)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c;
            return int.Parse(Math.Round(distance, 0).ToString());
        }

        public double radiano(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
