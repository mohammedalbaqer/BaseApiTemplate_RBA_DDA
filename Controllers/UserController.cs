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

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
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
            if (user == null) return NotFound();

            return Ok(UserMapper.ToResponseDto(user));
        }
    
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResponseDto<UserResponseDto>>> GetUsers([FromQuery] PaginationDto pagination)
        {
            var query = _userManager.Users;
            
            // Apply sorting if specified
            query = query.ApplySort(pagination.SortBy, pagination.IsDescending);

            var paginatedResult = await query.ToPaginatedListAsync(
                pagination.PageNumber,
                pagination.PageSize);

            var userDtos = paginatedResult.Items.Select(UserMapper.ToResponseDto);

            return Ok(new PaginatedResponseDto<UserResponseDto>
            {
                Items = userDtos,
                PageNumber = paginatedResult.PageNumber,
                PageSize = paginatedResult.PageSize,
                TotalPages = paginatedResult.TotalPages,
                TotalCount = paginatedResult.TotalCount
            });
        }
    
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserMapper.UpdateEntity(user, model);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return NoContent();

            return BadRequest(result.Errors);
        }
    
        [HttpPut("{id}/update-password")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdatePassword(string id, PasswordUpdateDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            var result = await _userManager.DeleteAsync(user);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpPost("{id}/profile-image")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UploadProfileImage(string id, [FromForm] ProfileImageDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            if (model.File == null)
            {
                return BadRequest("No image file provided.");
            }

            try
            {
                var (filePath, fileUrl) = await _fileService.SaveImageAsync(model.File, "profiles", user.ProfileImageUrl);
                user.ProfileImageUrl = filePath;
                
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(new { profileImageUrl = fileUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
