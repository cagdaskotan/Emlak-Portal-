using Microsoft.AspNetCore.Hosting;
using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using RealEstateApi.Repositories;
using System.Security.Claims;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ListingController : ControllerBase
    {
        private readonly GenericRepository<Listing> _listingRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        ResultDto _result = new ResultDto();

        public ListingController(IMapper mapper, GenericRepository<Listing> listingRepository, IWebHostEnvironment env)
        {
            _mapper = mapper;
            _listingRepository = listingRepository;
            _env = env;
        }

        [HttpGet]

        public async Task<List<ListingDto>> List()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var listings = await _listingRepository.Where(x => x.UserId == userId)
                                                   .Include(x => x.Category)
                                                   .Include(x => x.City)
                                                   .Include(x => x.Images)
                                                   .ToListAsync();

            var listingDtos = _mapper.Map<List<ListingDto>>(listings);
            return listingDtos;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] ListingCreateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var entity = new Listing
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                CategoryId = model.CategoryId,
                CityId = model.CityId,
                Address = model.Address,
                UserId = userId,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Images = new List<ListingImage>()
            };

            if (model.Files != null && model.Files.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                foreach (var file in model.Files)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    entity.Images.Add(new ListingImage
                    {
                        ImageUrl = $"{baseUrl}/img/{uniqueFileName}"
                    });
                }
            }

            await _listingRepository.AddAsync(entity);
            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla eklendi." });
        }

        [HttpPut]
        public async Task<ResultDto> Update([FromForm] ListingUpdateDto model)
        {
            var listing = await _listingRepository.GetAll()
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (listing == null)
                return new ResultDto { Status = false, Message = "İlan bulunamadı!" };

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (listing.UserId != userId)
                return new ResultDto { Status = false, Message = "Bu ilana erişim izniniz yok!" };

            // Sadece boş olmayan alanları güncelliyoruz
            if (!string.IsNullOrEmpty(model.Title))
                listing.Title = model.Title;

            if (!string.IsNullOrEmpty(model.Description))
                listing.Description = model.Description;

            if (model.Price.HasValue)
                listing.Price = model.Price.Value;

            if (model.CategoryId.HasValue)
                listing.CategoryId = model.CategoryId.Value;

            if (model.CityId.HasValue)
                listing.CityId = model.CityId.Value;

            if (!string.IsNullOrEmpty(model.Address))
                listing.Address = model.Address;

            listing.Updated = DateTime.Now;

            // Yeni görsel eklenmek isteniyorsa
            if (model.Files != null && model.Files.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                foreach (var file in model.Files)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var newImage = new ListingImage
                    {
                        ListingId = listing.Id,
                        ImageUrl = $"{baseUrl}/img/{uniqueFileName}"
                    };

                    await _listingRepository.Context.ListingImages.AddAsync(newImage);
                }
            }

            await _listingRepository.UpdateAsync(listing);
            await _listingRepository.Context.SaveChangesAsync();

            return new ResultDto { Status = true, Message = "İlan başarıyla güncellendi." };
        }


        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            var listing = await _listingRepository.GetAll()
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (listing == null)
                return new ResultDto { Status = false, Message = "İlan bulunamadı!" };

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (listing.UserId != userId)
                return new ResultDto { Status = false, Message = "Bu ilana erişim izniniz yok!" };

            foreach (var img in listing.Images)
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }
            }

            _listingRepository.Context.ListingImages.RemoveRange(listing.Images);
            await _listingRepository.DeleteAsync(id);
            await _listingRepository.Context.SaveChangesAsync();

            return new ResultDto { Status = true, Message = "İlan başarıyla silindi." };
        }

        [HttpGet("Public")]
        [AllowAnonymous]
        public async Task<List<ListingDto>> Public()
        {
            var listings = await _listingRepository.GetAll()
                .Include(x => x.Category)
                .Include(x => x.City)
                .Include(x => x.Images)
                .Include(x => x.User)
                .ToListAsync();

            var listingDtos = _mapper.Map<List<ListingDto>>(listings);
            return listingDtos;
        }

        [HttpDelete("DeleteImage/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _listingRepository.Context.ListingImages.FindAsync(id);
            if (image == null)
                return NotFound(new { status = false, message = "Görsel bulunamadı." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var listing = await _listingRepository.GetAll()
                .Where(l => l.Id == image.ListingId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (listing == null)
                return Forbid("Bu görseli silme izniniz yok.");

            var imagePath = Path.Combine(_env.WebRootPath, "img", Path.GetFileName(image.ImageUrl));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _listingRepository.Context.ListingImages.Remove(image);
            await _listingRepository.Context.SaveChangesAsync();

            return Ok(new { status = true, message = "Görsel başarıyla silindi." });
        }
    }
}
