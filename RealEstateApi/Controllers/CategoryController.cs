using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using RealEstateApi.Repositories;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly GenericRepository<Category> _categoryRepository;
        private readonly GenericRepository<Listing> _listingRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();

        public CategoryController(IMapper mapper, GenericRepository<Category> categoryRepository, GenericRepository<Listing> listingRepository)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _listingRepository = listingRepository;
        }

        [HttpGet]
        public async Task<List<CategoryDto>> List()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return categoryDtos;
        }

        [HttpGet("{id}")]
        public async Task<CategoryDto> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return categoryDto;
        }

        [HttpGet("{id}/Listings")]
        public async Task<List<ListingDto>> ListingList(int id)
        {
            var listings = await _listingRepository.Where(s => s.CategoryId == id).ToListAsync();
            var listingDtos = _mapper.Map<List<ListingDto>>(listings);
            return listingDtos;
        }

        [HttpPost]
        public async Task<ResultDto> Add(CategoryDto model)
        {
            var existingCategories = _categoryRepository.Where(s => s.Name == model.Name).ToList();
            if (existingCategories.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Girilen kategori adı zaten mevcut!";
                return _result;
            }

            var category = _mapper.Map<Category>(model);
            category.Created = DateTime.Now;
            category.Updated = DateTime.Now;
            await _categoryRepository.AddAsync(category);

            _result.Status = true;
            _result.Message = "Kategori başarıyla eklendi.";
            return _result;
        }

        [HttpPut]
        public async Task<ResultDto> Update(Category model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            if (category == null)
            {
                _result.Status = false;
                _result.Message = "Kategori bulunamadı!";
                return _result;
            }

            category.Name = model.Name;
            category.IsActive = model.IsActive;
            category.Updated = DateTime.Now;

            await _categoryRepository.UpdateAsync(category);

            _result.Status = true;
            _result.Message = "Kategori başarıyla güncellendi.";
            return _result;
        }

        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            var listings = await _listingRepository.Where(s => s.CategoryId == id).ToListAsync();
            if (listings.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Bu kategoriye bağlı ilanlar var, önce onları silmelisiniz!";
                return _result;
            }

            await _categoryRepository.DeleteAsync(id);

            _result.Status = true;
            _result.Message = "Kategori başarıyla silindi.";
            return _result;
        }
    }
}
