using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using MyIdentityApi.Models;
using MyIdentityApi.Dtos;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using MyIdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using MyIdentityApi.Mappers;
using MyIdentityApi.Dtos.User;
using MyIdentityApi.Dtos.Common;
using MyIdentityApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FileService _fileService;
    
        public UserController(UserManager<ApplicationUser> userManager, FileService fileService)
        {
            _userManager = userManager;
            _fileService = fileService;
        }
    
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);

            return CreateApiResponse<UserResponseDto>(UserMapper.ToResponseDto(user));
        }
    
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationDto pagination)
        {
            var query = _userManager.Users;
            
            // Apply search if specified
            if (!string.IsNullOrWhiteSpace(pagination.SearchQuery))
            {
                var searchTerm = pagination.SearchQuery.ToLower();
                query = query.Where(u => 
                    u.UserName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }
            
            // Apply sorting if specified
            query = query.ApplySort(pagination.SortBy, pagination.IsDescending);
    
            var count = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
    
            var userDtos = items.Select(UserMapper.ToResponseDto);
    
            var paginatedResponse = new PaginatedResponseDto<UserResponseDto>
            {
                Data = userDtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = count,
                TotalPages = (int)Math.Ceiling(count / (double)pagination.PageSize),
                SearchQuery = pagination.SearchQuery
            };
    
            return CreateApiResponse(paginatedResponse);
        }
    
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);

            UserMapper.UpdateEntity(user, model);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return CreateApiResponse<object>(null, 204);

            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpPut("{id}/update-password")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdatePassword(string id, PasswordUpdateDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded) return CreateApiResponse<object>(null, 204);

            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded) return CreateApiResponse<object>(null, 204);

            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpPost("{id}/profile-image")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UploadProfileImage(string id, [FromForm] ProfileImageDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);

            if (model.File == null)
            {
                return ApiError<object>(new List<string> { "No image file provided" });
            }

            try
            {
                var (filePath, fileUrl) = await _fileService.SaveImageAsync(model.File, "profiles", user.ProfileImageUrl);
                user.ProfileImageUrl = filePath;
                
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
                }

                return CreateApiResponse<object>(new { profileImageUrl = fileUrl });
            }
            catch (InvalidOperationException ex)
            {
                return ApiError<object>(new List<string> { ex.Message });
            }
        }
    }
}
