using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using AutoMapper;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public FavoriteController(AppDbContext context, UserManager<AppUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // Kullanıcının favori ilanlarını getir
        [HttpGet]
        public async Task<ActionResult<List<FavoriteDTO>>> Get()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var favorites = await _context.Favorites
                .Include(f => f.Listing)
                    .ThenInclude(l => l.Category)
                .Include(f => f.Listing)
                    .ThenInclude(l => l.City)
                .Include(f => f.Listing.Images)
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            var dtoList = _mapper.Map<List<FavoriteDTO>>(favorites);
            return Ok(dtoList);
        }

        // Favorilere ilan ekle
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] FavoriteCreateDTO dto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var exists = await _context.Favorites.AnyAsync(f => f.UserId == user.Id && f.ListingId == dto.ListingId);
            if (exists)
            {
                return BadRequest(new { status = false, message = "Bu ilan zaten favorilerinizde." });
            }

            var listingExists = await _context.Listings.AnyAsync(l => l.Id == dto.ListingId);
            if (!listingExists)
            {
                return BadRequest(new { status = false, message = "İlan bulunamadı." });
            }

            var favorite = new Favorite
            {
                UserId = user.Id,
                ListingId = dto.ListingId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Favorilere eklendi." });
        }

        // Favorilerden ilan sil
        [HttpDelete("{listingId}")]
        public async Task<IActionResult> Delete(int listingId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == user.Id && f.ListingId == listingId);
            if (favorite == null)
            {
                return NotFound(new { status = false, message = "Favori bulunamadı." });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Favoriden kaldırıldı." });
        }

        // Tüm favori kayıtlarını getir (Admin paneli için)
        [HttpGet("All")]
        public async Task<ActionResult<List<FavoriteDTO>>> GetAll()
        {
            var favorites = await _context.Favorites
                .Include(f => f.Listing)
                    .ThenInclude(l => l.Category)
                .Include(f => f.Listing)
                    .ThenInclude(l => l.City)
                .Include(f => f.Listing.Images)
                .Include(f => f.User)
                .ToListAsync();

            var dtoList = _mapper.Map<List<FavoriteDTO>>(favorites);
            return Ok(dtoList);
        }

        [HttpGet("MostFavoritedListings")]
        [AllowAnonymous]
        public async Task<IActionResult> MostFavoritedListings()
        {
            var favorites = await _context.Favorites
                .Include(f => f.Listing)
                    .ThenInclude(l => l.Category)
                .Include(f => f.Listing)
                    .ThenInclude(l => l.City)
                .Include(f => f.Listing.Images)
                .GroupBy(f => f.ListingId)
                .Select(group => new {
                    Listing = group.First().Listing,
                    FavoriteCount = group.Count()
                })
                .OrderByDescending(x => x.FavoriteCount)
                .Take(10)
                .ToListAsync();

            var listingDtos = favorites.Select(f => new ListingDto
            {
                Id = f.Listing.Id,
                Title = f.Listing.Title,
                Description = f.Listing.Description,
                Price = f.Listing.Price,
                Category = f.Listing.Category != null ? new CategoryDto { Name = f.Listing.Category.Name } : null,
                City = f.Listing.City != null ? new CityDTO { Name = f.Listing.City.Name } : null,
                Images = f.Listing.Images.Select(i => new ListingImageDto { ImageUrl = i.ImageUrl }).ToList()
            }).ToList();

            return Ok(listingDtos);
        }

    }
}
