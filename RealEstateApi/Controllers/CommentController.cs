using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.DTOs;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(AppDbContext context, IMapper mapper, UserManager<AppUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        // Belirli bir ilana ait yorumları getir
        [HttpGet("{listingId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDto>>> GetByListing(int listingId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ListingId == listingId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var dtos = _mapper.Map<List<CommentDto>>(comments);
            return Ok(dtos);
        }

        // Yorum ekle
        [HttpPost("Add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { status = false, message = "Yorum içeriği boş olamaz." });

            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user == null)
                return Unauthorized(new { status = false, message = "Kullanıcı doğrulanamadı." });

            var comment = new Comment
            {
                Content = dto.Content,
                ListingId = dto.ListingId,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Yorum başarıyla eklendi." });
        }

        // Yorum güncelle
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { status = false, message = "Yeni yorum içeriği boş olamaz." });

            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user == null) return Unauthorized();

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound(new { status = false, message = "Yorum bulunamadı." });

            if (comment.UserId != user.Id)
                return Forbid("Bu yorumu yalnızca sahibi düzenleyebilir.");

            comment.Content = dto.Content;
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Yorum güncellendi." });
        }

        // Yorum sil
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user == null) return Unauthorized();

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound(new { status = false, message = "Yorum bulunamadı." });

            if (comment.UserId != user.Id)
                return Forbid("Bu yorumu yalnızca sahibi silebilir.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Yorum silindi." });
        }

        // Tüm yorumları getir (Admin Panel için)
        [HttpGet("All")]
        [Authorize]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _context.Comments.ToListAsync();
            return Ok(comments);
        }
    }
}
