using lordran_archives.Data;
using lordran_archives.DTOs;
using lordran_archives.Models;
using lordran_archives.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LordranArchives.Tests
{
    [TestFixture]
    public class ItemServiceTests
    {
        private AppDbContext _db = null!;
        private ItemService _itemService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new AppDbContext(options);
            _itemService = new ItemService(_db);

            // Seed a test user
            _db.Users.Add(new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hash",
                Role = "user"
            });
            _db.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task GetApproved_ReturnsOnlyApprovedItems()
        {
            _db.Items.Add(new Item { Name = "Sword", Category = "weapon", Description = "A sword", Status = "approved", SubmittedById = 1 });
            _db.Items.Add(new Item { Name = "Shield", Category = "armor", Description = "A shield", Status = "pending", SubmittedById = 1 });
            _db.SaveChanges();

            var result = await _itemService.GetApproved();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Sword"));
        }

        [Test]
        public async Task GetPending_ReturnsOnlyPendingItems()
        {
            _db.Items.Add(new Item { Name = "Sword", Category = "weapon", Description = "A sword", Status = "approved", SubmittedById = 1 });
            _db.Items.Add(new Item { Name = "Shield", Category = "armor", Description = "A shield", Status = "pending", SubmittedById = 1 });
            _db.SaveChanges();

            var result = await _itemService.GetPending();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Shield"));
        }

        [Test]
        public async Task Submit_CreatesItemWithPendingStatus()
        {
            var result = await _itemService.Submit(new CreateItemDto
            {
                Name = "Estus Flask",
                Category = "consumable",
                Description = "Heals HP"
            }, 1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Status, Is.EqualTo("pending"));
        }

        [Test]
        public async Task Approve_ChangesStatusToApproved()
        {
            _db.Items.Add(new Item { Id = 1, Name = "Sword", Category = "weapon", Description = "A sword", Status = "pending", SubmittedById = 1 });
            _db.SaveChanges();

            var result = await _itemService.Approve(1);
            Assert.That(result, Is.True);

            var item = await _db.Items.FindAsync(1);
            Assert.That(item!.Status, Is.EqualTo("approved"));
        }

        [Test]
        public async Task Delete_RemovesItem()
        {
            _db.Items.Add(new Item { Id = 1, Name = "Sword", Category = "weapon", Description = "A sword", Status = "pending", SubmittedById = 1 });
            _db.SaveChanges();

            var result = await _itemService.Delete(1);
            Assert.That(result, Is.True);
            Assert.That(_db.Items.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetByCategory_ReturnsCorrectItems()
        {
            _db.Items.Add(new Item { Name = "Sword", Category = "weapon", Description = "A sword", Status = "approved", SubmittedById = 1 });
            _db.Items.Add(new Item { Name = "Ring", Category = "ring", Description = "A ring", Status = "approved", SubmittedById = 1 });
            _db.SaveChanges();

            var result = await _itemService.GetByCategory("weapon");
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Sword"));
        }
    }
}