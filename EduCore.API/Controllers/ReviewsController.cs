using EduCore.API.DTOs.Review;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _repo;

        public ReviewsController(IReviewRepository repo)
        {
            _repo = repo;
        }

        // =========================
        // PUBLIC / STUDENT
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetFeaturedAsync()
        {
            var result = await _repo.GetFeaturedAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(Guid courseId)
        {
            var result = await _repo.GetByCourseAsync(courseId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromQuery] Guid studentId,
            [FromBody] CreateReviewRequest req)
        {
            var result = await _repo.CreateAsync(studentId, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(
            Guid id,
            [FromQuery] Guid studentId,
            [FromBody] UpdateReviewRequest req)
        {
            var result = await _repo.UpdateAsync(id, studentId, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(
            Guid id,
            [FromQuery] Guid studentId)
        {
            var result = await _repo.DeleteAsync(id, studentId);
            return StatusCode(result.StatusCode, result);
        }

        // =========================
        // ADMIN
        // =========================

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] ReviewAdminQuery query)
        {
            var result = await _repo.GetAllForAdminAsync(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("admin/{id}/approve")]
        public async Task<IActionResult> ApproveAsync(Guid id)
        {
            var result = await _repo.ApproveAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("admin/{id}/featured")]
        public async Task<IActionResult> ToggleFeaturedAsync(Guid id)
        {
            var result = await _repo.ToggleFeaturedAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> AdminDeleteAsync(Guid id)
        {
            var result = await _repo.AdminDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/stats")]
        public async Task<IActionResult> GetStatsAsync()
        {
            var result = await _repo.GetReviewStatsAsync();
            return StatusCode(result.StatusCode, result);
        }
    }
}