using TrybeHotel.Models;
using TrybeHotel.Dto;

namespace TrybeHotel.Repository
{
    public class HotelRepository : IHotelRepository
    {
        protected readonly ITrybeHotelContext _context;
        public HotelRepository(ITrybeHotelContext context)
        {
            _context = context;
        }

        //  5. Refatore o endpoint GET /hotel
        public IEnumerable<HotelDto> GetHotels()
        {
            var hotels = _context.Hotels
                .Select(h => new HotelDto
                {
                    hotelId = h.HotelId,
                    name = h.Name,
                    address = h.Address,
                    cityId = h.CityId,
                    cityName = h.City.Name,
                    state = h.City.State
                });
            return hotels;
        }

        // 6. Refatore o endpoint POST /hotel
        public HotelDto AddHotel(Hotel hotel)
        {
            var newHotel = _context.Cities.Find(hotel.CityId);
            _context.Hotels.Add(hotel);
            _context.SaveChanges();
            return new HotelDto
            {
                hotelId = hotel.HotelId,
                name = hotel.Name,
                address = hotel.Address,
                cityId = hotel.CityId,
                cityName = hotel.City.Name,
                state = hotel.City.State
            };
        }
    }
}