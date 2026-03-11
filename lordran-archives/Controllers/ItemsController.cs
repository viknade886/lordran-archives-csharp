using System.Security.Claims;
using lordran_archives.DTOs;
using lordran_archives.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lordran_archives.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ItemService _itemService;

        public ItemsController(ItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _itemService.GetApproved());

        [HttpGet("category/{cat}")]
        public async Task<IActionResult> GetByCategory(string cat) =>
            Ok(await _itemService.GetByCategory(cat));

        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPending()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "admin" && role != "operator")
                return StatusCode(403, new { message = "Forbidden" });
            return Ok(await _itemService.GetPending());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Submit(CreateItemDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var result = await _itemService.Submit(dto, userId);
            return Ok(result);
        }

        [HttpPut("approve/{id}")]
        [Authorize]
        public async Task<IActionResult> Approve(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "admin" && role != "operator")
                return StatusCode(403, new { message = "Forbidden" });
            var result = await _itemService.Approve(id);
            if (!result) return NotFound();
            return Ok(new { message = "Approved" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "admin" && role != "operator")
                return StatusCode(403, new { message = "Forbidden" });
            var result = await _itemService.Delete(id);
            if (!result) return NotFound();
            return Ok(new { message = "Deleted" });
        }
    }
}