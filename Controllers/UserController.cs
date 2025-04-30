using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using MyIdentityApi.Models;
using MyIdentityApi.Dtos;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
    
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
    
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            return Ok(user);
        }
    
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            return Ok(users);
        }
    
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            user.Email = model.Email;
            user.UserName = model.Username;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
    
            var result = await _userManager.UpdateAsync(user);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpPut("{id}/update-password")]
        public async Task<IActionResult> UpdatePassword(string id, PasswordUpdateDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
    
            var result = await _userManager.DeleteAsync(user);
    
            if (result.Succeeded) return NoContent();
    
            return BadRequest(result.Errors);
        }
    }

}
