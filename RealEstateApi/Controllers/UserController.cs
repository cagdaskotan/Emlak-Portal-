using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;

namespace RealEstateApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        ResultDto result = new ResultDto();

        public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration configuration, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<List<UserDto>> List()
        {
            var users = _userManager.Users.ToList();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var userDto in userDtos)
            {
                var user = await _userManager.FindByIdAsync(userDto.Id);
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Role = roles.FirstOrDefault() ?? "Uye";

                userDto.UserPhoto = string.IsNullOrEmpty(userDto.UserPhoto)
                    ? $"{baseUrl}/UserPhotos/user.png"
                    : $"{baseUrl}{userDto.UserPhoto}";
            }

            return userDtos;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<UserDto> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);

            // Rol bilgisini ayrıca getiriyoruz
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault() ?? "Uye";

            // Profil fotoğrafı linki tam gelsin
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            userDto.UserPhoto = string.IsNullOrEmpty(userDto.UserPhoto)
                ? $"{baseUrl}/UserPhotos/user.png"
                : $"{baseUrl}{userDto.UserPhoto}";

            return userDto;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> Register([FromForm] RegisterDto dto, [FromForm] IFormFile? userPhoto)
        {
            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                UserPhoto = "/UserPhotos/user.png"
            };

            if (userPhoto != null && userPhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserPhotos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(userPhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await userPhoto.CopyToAsync(stream);

                user.UserPhoto = $"/UserPhotos/{uniqueFileName}";
            }

            var identityResult = await _userManager.CreateAsync(user, dto.Password);
            if (!identityResult.Succeeded)
            {
                result.Status = false;
                foreach (var error in identityResult.Errors)
                    result.Message += $"<p>{error.Description}</p>";
                return result;
            }

            if (!await _roleManager.RoleExistsAsync("Uye"))
                await _roleManager.CreateAsync(new AppRole { Name = "Uye" });

            await _userManager.AddToRoleAsync(user, "Uye");

            result.Status = true;
            result.Message = "Kullanıcı başarıyla kaydedildi.";
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> Login(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return new ResultDto { Status = false, Message = "Kullanıcı bulunamadı!" };

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre yanlış!" };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateJwtToken(claims);
            var userRoles = await _userManager.GetRolesAsync(user);

            var responseObject = new
            {
                token,
                userName = user.UserName,
                role = userRoles.FirstOrDefault() ?? "Uye",
                userId = user.Id
            };

            return new ResultDto
            {
                Status = true,
                Message = System.Text.Json.JsonSerializer.Serialize(responseObject)
            };
        }


        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["AccessTokenExpiration"])),
                claims: claims,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPut]
        public async Task<ResultDto> Update([FromForm] UserUpdateDto model, [FromForm] IFormFile? userPhoto)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return new ResultDto { Status = false, Message = "Kullanıcı bulunamadı!" };

            if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.Email)) user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.UserName)) user.UserName = model.UserName;
            if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber;

            if (userPhoto != null && userPhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.UserPhoto) && user.UserPhoto != "/UserPhotos/user.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.UserPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserPhotos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(userPhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await userPhoto.CopyToAsync(stream);

                user.UserPhoto = $"/UserPhotos/{uniqueFileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Status = false;
                foreach (var error in updateResult.Errors)
                    result.Message += $"<p>{error.Description}</p>";
                return result;
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any()) await _userManager.RemoveFromRolesAsync(user, roles);

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                    await _roleManager.CreateAsync(new AppRole { Name = model.Role });

                await _userManager.AddToRoleAsync(user, model.Role);
            }

            result.Status = true;
            result.Message = "Kullanıcı bilgileri ve rol başarıyla güncellendi.";
            return result;
        }

        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return new ResultDto { Status = false, Message = "Kullanıcı bulunamadı!" };

            var resultDelete = await _userManager.DeleteAsync(user);
            if (!resultDelete.Succeeded)
            {
                result.Status = false;
                foreach (var error in resultDelete.Errors)
                    result.Message += $"<p>{error.Description}</p>";
                return result;
            }

            result.Status = true;
            result.Message = "Kullanıcı başarıyla silindi.";
            return result;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDto model, [FromForm] IFormFile? userPhoto)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound(new { status = false, message = "Kullanıcı bulunamadı." });

            if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.Email)) user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.UserName)) user.UserName = model.UserName;
            if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber;

            if (userPhoto != null && userPhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.UserPhoto) && user.UserPhoto != "/UserPhotos/user.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.UserPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserPhotos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(userPhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await userPhoto.CopyToAsync(stream);

                user.UserPhoto = $"/UserPhotos/{uniqueFileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(new { status = false, message = "Kullanıcı güncellenemedi." });

            return Ok(new { status = true, message = "Profil başarıyla güncellendi." });
        }

    }
}
