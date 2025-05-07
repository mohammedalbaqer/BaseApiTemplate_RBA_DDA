using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using MyIdentityApi.Dtos;
using MyIdentityApi.Models;
using MyIdentityApi.Dtos.Role;
using MyIdentityApi.Mappers;

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
    
        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
    
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
    
            return Ok(RoleMapper.ToResponseDto(role));
        }
    
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles.Select(RoleMapper.ToResponseDto));
        }
    
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(CreateRoleDto model)
        {
            var role = RoleMapper.ToEntity(model);
            var result = await _roleManager.CreateAsync(role);
    
            if (result.Succeeded) return Ok(RoleMapper.ToResponseDto(role));
    
            return BadRequest(result.Errors);
        }
    
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(string id, UpdateRoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
    
            RoleMapper.UpdateEntity(role, model);
            var result = await _roleManager.UpdateAsync(role);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
    
            var result = await _roleManager.DeleteAsync(role);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpPost("{roleId}/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserToRole(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
    
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();
    
            var result = await _userManager.AddToRoleAsync(user, role.Name);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpDelete("{roleId}/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromRole(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
    
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();
    
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    }

}
