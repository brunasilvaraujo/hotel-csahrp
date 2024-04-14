namespace TrybeHotel.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TrybeHotel.Models;
using TrybeHotel.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;
using TrybeHotel.Dto;
using TrybeHotel.Services;
using System.Net.Http.Headers;
using System.Net;

public class TokenDto
{
    public string? token { get; set; }
}


public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private string? _jwtToken = null;
    public HttpClient _clientTest;

    public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        //_factory = factory;
        _clientTest = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TrybeHotelContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContextTest>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDatabase");
                });
                services.AddScoped<ITrybeHotelContext, ContextTest>();
                services.AddScoped<ICityRepository, CityRepository>();
                services.AddScoped<IHotelRepository, HotelRepository>();
                services.AddScoped<IRoomRepository, RoomRepository>();
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                using (var appContext = scope.ServiceProvider.GetRequiredService<ContextTest>())
                {
                    appContext.Database.EnsureCreated();
                    appContext.Database.EnsureDeleted();
                    appContext.Database.EnsureCreated();
                    appContext.Cities.Add(new City { CityId = 1, Name = "Manaus", State = "AM" });
                    appContext.Cities.Add(new City { CityId = 2, Name = "Palmas", State = "TO" });
                    appContext.SaveChanges();
                    appContext.Hotels.Add(new Hotel { HotelId = 1, Name = "Trybe Hotel Manaus", Address = "Address 1", CityId = 1 });
                    appContext.Hotels.Add(new Hotel { HotelId = 2, Name = "Trybe Hotel Palmas", Address = "Address 2", CityId = 2 });
                    appContext.Hotels.Add(new Hotel { HotelId = 3, Name = "Trybe Hotel Ponta Negra", Address = "Addres 3", CityId = 1 });
                    appContext.SaveChanges();
                    appContext.Rooms.Add(new Room { RoomId = 1, Name = "Room 1", Capacity = 2, Image = "Image 1", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 2, Name = "Room 2", Capacity = 3, Image = "Image 2", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 3, Name = "Room 3", Capacity = 4, Image = "Image 3", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 4, Name = "Room 4", Capacity = 2, Image = "Image 4", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 5, Name = "Room 5", Capacity = 3, Image = "Image 5", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 6, Name = "Room 6", Capacity = 4, Image = "Image 6", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 7, Name = "Room 7", Capacity = 2, Image = "Image 7", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 8, Name = "Room 8", Capacity = 3, Image = "Image 8", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 9, Name = "Room 9", Capacity = 4, Image = "Image 9", HotelId = 3 });
                    appContext.SaveChanges();
                    appContext.Users.Add(new User { UserId = 1, Name = "Ana", Email = "ana@trybehotel.com", Password = "Senha1", UserType = "admin" });
                    appContext.Users.Add(new User { UserId = 2, Name = "Beatriz", Email = "beatriz@trybehotel.com", Password = "Senha2", UserType = "client" });
                    appContext.Users.Add(new User { UserId = 3, Name = "Laura", Email = "laura@trybehotel.com", Password = "Senha3", UserType = "client" });
                    appContext.SaveChanges();
                    appContext.Bookings.Add(new Booking { BookingId = 1, CheckIn = new DateTime(2023, 07, 02), CheckOut = new DateTime(2023, 07, 03), GuestQuant = 1, UserId = 2, RoomId = 1 });
                    appContext.Bookings.Add(new Booking { BookingId = 2, CheckIn = new DateTime(2023, 07, 02), CheckOut = new DateTime(2023, 07, 03), GuestQuant = 1, UserId = 3, RoomId = 4 });
                    appContext.SaveChanges();
                }
            });
        }).CreateClient();
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Executando meus testes")]
    [InlineData("/city")]
    public async Task TestGet(string url)
    {
        var response = await _clientTest.GetAsync(url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);
    }

    [Theory(DisplayName = "Teste add booking")]
    [InlineData("/booking")]

    public async Task TestPostBooking(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Content = new StringContent(JsonConvert.SerializeObject(new Booking { BookingId = 3, CheckIn = new DateTime(2023, 07, 02), CheckOut = new DateTime(2023, 07, 03), GuestQuant = 1, UserId = 2, RoomId = 1 }), Encoding.UTF8, "application/json");
        var response = await _clientTest.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory(DisplayName = "Teste get room")]
    [InlineData("/room")]
    public async Task TestGetRomm(string url)
    {
        var response = await _clientTest.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }

    [Theory(DisplayName = "Teste de busca de quarto por Id")]
    [InlineData("/room/1")]
    public async Task TestGetRoom(string url)
    {
        var response = await _clientTest.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response?.StatusCode);
    }

    [Theory(DisplayName = "Teste get booking")]
    [InlineData("/bookeng/1")]
    public async Task TestGetBooking(string url)
    {
        var response = await _clientTest.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory(DisplayName = "Teste de login jwt")]
    [InlineData("/login")]
    public async Task TestLogin(string url)
    {
        UserDto userMoq = new()
        {
            UserId = 1,
            Email = "ana@trybehotel.com",
            Password = "Senha1",
            UserType = "admin"
        };

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(userMoq), Encoding.UTF8, "application/json")
        };

        var response = await _clientTest.SendAsync(loginRequest);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var tokenDto = JsonConvert.DeserializeObject<TokenDto>(responseString);

        Assert.NotNull(tokenDto);
        Assert.NotNull(tokenDto.Token);

        _jwtToken = tokenDto.Token;
    }

    [Theory(DisplayName = "Teste add hotel")]
    [InlineData("/hotel")]
    public async Task TestAddHotel(string url)
    {
        await TestLogin("/login");

        var hotel = new Hotel { Name = "Nova Cidade", Address = "Endereço", CityId = 1 };
        var content = new StringContent(JsonConvert.SerializeObject(hotel), Encoding.UTF8, "application/json");

        var hotelRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        hotelRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

        var response = await _clientTest.SendAsync(hotelRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory(DisplayName = "Teste add room")]
    [InlineData("/room")]
    public async Task TestAddRoom(string url)
    {
        await TestLogin("/login");

        var room = new Room { Name = "Room 1", Capacity = 2, Image = "image.jpg", HotelId = 1 };
        var content = new StringContent(JsonConvert.SerializeObject(room), Encoding.UTF8, "application/json");

        var roomRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        roomRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

        var response = await _clientTest.SendAsync(roomRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory(DisplayName = "Teste add city")]
    [InlineData("/city")]
    public async Task TestAddCity(string url)
    {
        await TestLogin("/login");

        var city = new City { Name = "New City" };
        var content = new StringContent(JsonConvert.SerializeObject(city), Encoding.UTF8, "application/json");

        var cityRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        cityRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

        var response = await _clientTest.SendAsync(cityRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private class TokenDto
    {
        public string? Token { get; set; }
    }

}