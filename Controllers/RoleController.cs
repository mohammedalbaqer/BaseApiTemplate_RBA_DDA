using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using MyIdentityApi.Dtos;
using MyIdentityApi.Models;
using MyIdentityApi.Dtos.Role;
using MyIdentityApi.Mappers;
using MyIdentityApi.Dtos.Common;
using MyIdentityApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseApiController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
    
        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
    
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles([FromQuery] PaginationDto pagination)
        {
            var query = _roleManager.Roles;
            
            if (!string.IsNullOrWhiteSpace(pagination.SearchQuery))
            {
                var searchTerm = pagination.SearchQuery.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(searchTerm));
            }
            
            query = query.ApplySort(pagination.SortBy, pagination.IsDescending);
    
            var count = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
    
            var roleDtos = items.Select(RoleMapper.ToResponseDto);
    
            var paginatedResponse = new PaginatedResponseDto<RoleResponseDto>
            {
                Data = roleDtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = count,
                TotalPages = (int)Math.Ceiling(count / (double)pagination.PageSize),
                SearchQuery = pagination.SearchQuery
            };
    
            return CreateApiResponse(paginatedResponse);
        }
    
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return ApiError<object>(new List<string> { "Role not found" }, 404);
    
            return CreateApiResponse(RoleMapper.ToResponseDto(role));
        }
    
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(CreateRoleDto model)
        {
            var role = RoleMapper.ToEntity(model);
            var result = await _roleManager.CreateAsync(role);
    
            if (result.Succeeded) return CreateApiResponse(RoleMapper.ToResponseDto(role), 201);
    
            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(string id, UpdateRoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return ApiError<object>(new List<string> { "Role not found" }, 404);
    
            RoleMapper.UpdateEntity(role, model);
            var result = await _roleManager.UpdateAsync(role);
    
            if (result.Succeeded) return CreateApiResponse<object>(null, 204);
    
            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return ApiError<object>(new List<string> { "Role not found" }, 404);
    
            var result = await _roleManager.DeleteAsync(role);
    
            if (result.Succeeded) return CreateApiResponse<object>(null, 204);
    
            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpPost("{roleId}/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserToRole(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);
    
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return ApiError<object>(new List<string> { "Role not found" }, 404);
    
            var result = await _userManager.AddToRoleAsync(user, role.Name);
    
            if (result.Succeeded) return CreateApiResponse<object>(null, 204);
    
            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    
        [HttpDelete("{roleId}/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromRole(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 404);
    
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return ApiError<object>(new List<string> { "Role not found" }, 404);
    
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
    
            if (result.Succeeded) return CreateApiResponse<object>(null, 204);
    
            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }
    }
}
