using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.DTOs;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CityController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CityDTO>>> GetCities()
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            var dtos = _mapper.Map<List<CityDTO>>(cities);
            return Ok(dtos);
        }
    }
}
