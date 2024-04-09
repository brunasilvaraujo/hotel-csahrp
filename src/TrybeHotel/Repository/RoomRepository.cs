using TrybeHotel.Models;
using TrybeHotel.Dto;

namespace TrybeHotel.Repository
{
    public class RoomRepository : IRoomRepository
    {
        protected readonly ITrybeHotelContext _context;
        public RoomRepository(ITrybeHotelContext context)
        {
            _context = context;
        }

        // 7. Refatore o endpoint GET /room
        public IEnumerable<RoomDto> GetRooms(int HotelId)
        {
            var rooms = _context.Rooms
            .Where(room => room.HotelId == HotelId)
            .Join(_context.Hotels,
                room => room.HotelId,
                hotel => hotel.HotelId,
                (room, hotel) => new RoomDto
                {
                    roomId = room.RoomId,
                    name = room.Name,
                    capacity = room.Capacity,
                    image = room.Image,
                    hotel = new HotelDto
                    {
                        hotelId = hotel.HotelId,
                        name = hotel.Name,
                        address = hotel.Address,
                        cityId = hotel.CityId,
                        cityName = hotel.City.Name,
                        state = hotel.City.State
                    }
                });
            return rooms;
        }

        // 8. Refatore o endpoint POST /room
        public RoomDto AddRoom(Room room)
        {
            var newRoom = _context.Rooms.Add(room);
            _context.SaveChanges();
            var newHotel = _context.Hotels.Find(room.HotelId);
            var city = _context.Cities.FirstOrDefault(c => c.CityId == newHotel.CityId);
            return new RoomDto
            {
                roomId = newRoom.Entity.RoomId,
                name = newRoom.Entity.Name,
                capacity = newRoom.Entity.Capacity,
                image = newRoom.Entity.Image,
                hotel = new HotelDto
                {
                    hotelId = newHotel.HotelId,
                    name = newHotel.Name,
                    address = newHotel.Address,
                    cityId = city.CityId,
                    cityName = city.Name,
                    state = city.State
                }
            };

        }

        public void DeleteRoom(int RoomId)
        {
            var room = _context.Rooms.Find(RoomId);
            _context.Rooms.Remove(room);
            _context.SaveChanges();
        }
    }
}