using EduCore.API.DTOs.Course;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _repo;
        public CourseController(ICourseRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _repo.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _repo.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(string key, int page = 1, int pageSize = 10)
        {
            var result = await _repo.SearchAsync(key, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var result = await _repo.GetBySlugAsync(slug);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterAsync(string? level, string? price, bool? isPublished, int page = 1, int pageSize = 10)
        {
            var result = await _repo.FilterAsync(level, price, isPublished, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateAsync([FromForm] CreateCourseRequest req)
        {
            var result = await _repo.CreateAsync(req.instructorId, req.Title, req.Slug!, req.Description, req.Content, req.Thumbnail, req.PreviewVideo, req.Price, req.Level, req.Language, req.CategoryId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] UpdateCourseRequest req)
        {
            var result = await _repo.UpdateAsync(id, req.InstructorId, req.Title, req.Slug, req.Description, req.Content, req.Thumbnail, req.PreviewVideo, req.Price, req.Level, req.Language, req.CategoryId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id}/toggle-publish")]
        public async Task<IActionResult> TogglePublishAsync(Guid id)
        {
            var result = await _repo.TogglePublishAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("toggle-hot/{id}")]
        public async Task<IActionResult> ToggleHotAsync(Guid id)
        {
            var result = await _repo.ToggleHotAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _repo.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
