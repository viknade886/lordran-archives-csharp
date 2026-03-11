using lordran_archives.Data;
using lordran_archives.DTOs;
using lordran_archives.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LordranArchives.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AppDbContext _db = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new AppDbContext(options);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Secret", "ArtoriasOfTheAbyssWalkerOfTheAbyss" }
                })
                .Build();

            _authService = new AuthService(_db, config);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task Register_NewUser_ReturnsSuccess()
        {
            var result = await _authService.Register(new RegisterDto
            {
                Username = "testuser",
                Password = "password123"
            });
            Assert.That(result, Is.EqualTo("User created"));
        }

        [Test]
        public async Task Register_DuplicateUsername_ReturnsNull()
        {
            await _authService.Register(new RegisterDto { Username = "testuser", Password = "password123" });
            var result = await _authService.Register(new RegisterDto { Username = "testuser", Password = "password456" });
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Login_CorrectCredentials_ReturnsToken()
        {
            await _authService.Register(new RegisterDto { Username = "testuser", Password = "password123" });
            var result = await _authService.Login(new LoginDto { Username = "testuser", Password = "password123" });
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Token, Is.Not.Empty);
        }

        [Test]
        public async Task Login_WrongPassword_ReturnsNull()
        {
            await _authService.Register(new RegisterDto { Username = "testuser", Password = "password123" });
            var result = await _authService.Login(new LoginDto { Username = "testuser", Password = "wrongpassword" });
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Login_NonExistentUser_ReturnsNull()
        {
            var result = await _authService.Login(new LoginDto { Username = "nobody", Password = "password123" });
            Assert.That(result, Is.Null);
        }
    }
}