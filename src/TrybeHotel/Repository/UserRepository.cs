using TrybeHotel.Models;
using TrybeHotel.Dto;

namespace TrybeHotel.Repository
{
    public class UserRepository : IUserRepository
    {
        protected readonly ITrybeHotelContext _context;
        public UserRepository(ITrybeHotelContext context)
        {
            _context = context;
        }
        public UserDto GetUserById(int userId)
        {
            throw new NotImplementedException();
        }

        public UserDto Login(LoginDto login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);
            if (user == null)
            {
                throw new Exception("Incorrect e-mail or password");
            }
            return new UserDto
            {
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                userType = user.UserType
            };
        }
        public UserDto Add(UserDtoInsert user)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.email);
                if (existingUser != null)
                {
                    throw new Exception("User email already exists");
                }
                var newUser = new User
                {
                    Name = user.name,
                    Email = user.email,
                    Password = user.password,
                    UserType = "client"
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return new UserDto
                {
                    userId = newUser.UserId,
                    name = newUser.Name,
                    email = newUser.Email,
                    userType = newUser.UserType
                };
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public UserDto GetUserByEmail(string userEmail)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserDto> GetUsers()
        {
            try
            {
                var users = _context.Users.Select(u => new UserDto
                {
                    userId = u.UserId,
                    name = u.Name,
                    email = u.Email,
                    userType = u.UserType
                });
                return users;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}