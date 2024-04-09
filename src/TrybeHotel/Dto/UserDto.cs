namespace TrybeHotel.Dto
{
    public class UserDto
    {
        public int userId { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? userType { get; set; }
        public string? Password { get; set; }
    }

    public class UserDtoInsert
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
    }

    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}