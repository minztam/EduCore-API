using EduCore.API.DTOs.AuthDTO;
using EduCore.API.DTOs.User;
using EduCore.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetPagedUsersAsync([FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _repo.GetPagedUsersAsync(pageIndex, pageSize, keyword, role, isActive);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(string email, string password, string name)
        {
            var result =  await _repo.RegisterAsync(email, password, name);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            var result = await _repo.LoginAsync(req.Email, req.Password);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequest req)
        {
            var result = await _repo.GoogleLoginAsync(req.Token);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("search")]
        public async Task<IActionResult> SearchByNameAsync([FromQuery]string key)
        {
            var result = await _repo.SearchByNameAsync(key);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("statistics")]
        public async Task<IActionResult> GetUserStatisticsAsync()
        {
            var result = await _repo.GetUserStatisticsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatusAsync(Guid id)
        {
            var result = await _repo.ToggleUserStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateUserProfile(Guid id,[FromForm] UpdateUserProfileRequest req)
        {
            var result = await _repo.UpdateUserProfileAsync(id,req.Name,req.Avata,req.Password);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var result = await _repo.GetUserByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRoleAsync(Guid id, [FromBody] string newRole)
        {
            var result = await _repo.ChangeRoleAsync(id, newRole);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            var result = await _repo.DeleteUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("bulk-toggle")]
        public async Task<IActionResult> BulkToggleStatusAsync([FromBody] List<Guid> ids)
        {
            var result = await _repo.BulkToggleStatusAsync(ids);
            return StatusCode(result.StatusCode, result);
        }
    }
}
