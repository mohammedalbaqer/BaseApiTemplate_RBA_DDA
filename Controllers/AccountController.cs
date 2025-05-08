using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using MyIdentityApi.Dtos.Account;
using MyIdentityApi.Services;
using MyIdentityApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MyIdentityApi.Data;
using MyIdentityApi.Mappers;

namespace MyIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly RefreshTokenService _refreshTokenService;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService jwtTokenService,
            RefreshTokenService refreshTokenService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var user = AccountMapper.ToEntity(model);
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                var (token, expiresAt) = await _jwtTokenService.GenerateToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);
                
                return CreateApiResponse(AccountMapper.ToAuthResponseDto(token, expiresAt, refreshToken.Token), 201);
            }

            return ApiError<object>(result.Errors.Select(e => e.Description).ToList());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null) return ApiError<object>(new List<string> { "User not found" }, 401);

                var (token, expiresAt) = await _jwtTokenService.GenerateToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);
                
                return CreateApiResponse(AccountMapper.ToAuthResponseDto(token, expiresAt, refreshToken.Token));
            }

            return ApiError<object>(new List<string> { "Invalid username or password" }, 401);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto tokenDto)
        {
            ArgumentNullException.ThrowIfNull(tokenDto);

            if (!await _refreshTokenService.IsTokenValidAsync(tokenDto.RefreshToken))
            {
                return ApiError<object>(new List<string> { "Invalid refresh token" }, 401);
            }

            var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(tokenDto.RefreshToken);
            if (refreshToken == null) return ApiError<object>(new List<string> { "Refresh token not found" }, 401);

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null) return ApiError<object>(new List<string> { "User not found" }, 401);

            var (newToken, expiresAt) = await _jwtTokenService.GenerateToken(user);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);

            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);

            return CreateApiResponse(AccountMapper.ToAuthResponseDto(newToken, expiresAt, newRefreshToken.Token));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return ApiError<object>(new List<string> { "Unauthorized" }, 401);

                await _refreshTokenService.RevokeAllUserTokensAsync(userId);
                await _signInManager.SignOutAsync();
                Response.Cookies.Delete(".AspNetCore.Identity.Application");

                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var revokedToken = RevokedTokenMapper.ToEntity(accessToken, userId);
                    _context.RevokedTokens.Add(revokedToken);
                    await _context.SaveChangesAsync();
                }

                Response.Headers.Add("Token-Expired", "true");
                Response.Headers["WWW-Authenticate"] = "Bearer error=\"token_revoked\"";

                return CreateApiResponse(new { message = "Logged out successfully" });
            }
            catch (Exception)
            {
                return ApiError<object>(new List<string> { "An error occurred during logout" }, 500);
            }
        }
    }
}



