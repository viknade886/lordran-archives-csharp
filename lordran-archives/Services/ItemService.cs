using lordran_archives.Data;
using lordran_archives.DTOs;
using lordran_archives.Models;
using Microsoft.EntityFrameworkCore;

namespace lordran_archives.Services
{
    public class ItemService
    {
        private readonly AppDbContext _db;

        public ItemService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ItemResponseDto>> GetApproved()
        {
            return await _db.Items
                .Where(i => i.Status == "approved")
                .Include(i => i.SubmittedBy)
                .Select(i => ToDto(i))
                .ToListAsync();
        }

        public async Task<List<ItemResponseDto>> GetPending()
        {
            return await _db.Items
                .Where(i => i.Status == "pending")
                .Include(i => i.SubmittedBy)
                .Select(i => ToDto(i))
                .ToListAsync();
        }

        public async Task<List<ItemResponseDto>> GetByCategory(string category)
        {
            return await _db.Items
                .Where(i => i.Status == "approved" && i.Category == category)
                .Include(i => i.SubmittedBy)
                .Select(i => ToDto(i))
                .ToListAsync();
        }

        public async Task<ItemResponseDto?> Submit(CreateItemDto dto, int userId)
        {
            var item = new Item
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                Image = dto.Image,
                SubmittedById = userId
            };

            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            var saved = await _db.Items.Include(i => i.SubmittedBy).FirstAsync(i => i.Id == item.Id);
            return ToDto(saved);
        }

        public async Task<bool> Approve(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return false;
            item.Status = "approved";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return false;
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        private static ItemResponseDto ToDto(Item i) => new ItemResponseDto
        {
            Id = i.Id,
            Name = i.Name,
            Category = i.Category,
            Description = i.Description,
            Image = i.Image,
            Status = i.Status,
            SubmittedBy = i.SubmittedBy.Username,
            CreatedAt = i.CreatedAt
        };
    }
}