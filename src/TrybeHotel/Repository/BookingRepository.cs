using TrybeHotel.Models;
using TrybeHotel.Dto;
using Microsoft.EntityFrameworkCore;

namespace TrybeHotel.Repository
{
    public class BookingRepository : IBookingRepository
    {
        protected readonly ITrybeHotelContext _context;
        public BookingRepository(ITrybeHotelContext context)
        {
            _context = context;
        }

        // 9. Refatore o endpoint POST /booking
        public BookingResponse Add(BookingDtoInsert booking, string email)
        {
            var user = _context.Users.First(u => u.Email == email);
            var room = _context.Rooms.Find(booking.RoomId);
            if (room == null || booking.GuestQuant > room.Capacity)
            {
                throw new Exception("Guest quantity over room capacity");
            }
            var hotel = _context.Hotels.Find(room.HotelId);
            var city = _context.Cities.FirstOrDefault(c => c.CityId == hotel.CityId);
            var newBooking = new Booking
            {
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                GuestQuant = booking.GuestQuant,
                RoomId = booking.RoomId,
                UserId = user.UserId
            };

            var entity = _context.Bookings.Add(newBooking);
            _context.SaveChanges();

            return new BookingResponse
            {
                BookingId = entity.Entity.BookingId,
                CheckIn = entity.Entity.CheckIn,
                CheckOut = entity.Entity.CheckOut,
                GuestQuant = entity.Entity.GuestQuant,
                Room = new RoomDto
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
                        cityId = city.CityId,
                        cityName = city.Name
                    }
                }
            };
        }

        // 10. Refatore o endpoint GET /booking
        public BookingResponse GetBooking(int bookingId, string email)
        {
            var booking = _context.Bookings
               .Include(b => b.Room)
               .ThenInclude(r => r.Hotel)
               .ThenInclude(h => h.City)
               .FirstOrDefault(b => b.BookingId == bookingId);
            var user = _context.Users.First(u => u.Email == email);

            if (booking.UserId != user.UserId)
            {
                throw new Exception("Unauthorized access");
            }
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                GuestQuant = booking.GuestQuant,
                Room = new RoomDto
                {
                    roomId = booking.Room.RoomId,
                    name = booking.Room.Name,
                    capacity = booking.Room.Capacity,
                    image = booking.Room.Image,
                    hotel = new HotelDto
                    {
                        hotelId = booking.Room.Hotel.HotelId,
                        name = booking.Room.Hotel.Name,
                        address = booking.Room.Hotel.Address,
                        cityId = booking.Room.Hotel.CityId,
                        cityName = booking.Room.Hotel.City.Name
                    }
                }
            };
        }

        public Room GetRoomById(int RoomId)
        {
            throw new NotImplementedException();
        }

    }

}