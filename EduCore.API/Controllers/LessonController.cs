using EduCore.API.DTOs.Lesson;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly ILessonRepository _repo;
        public LessonController(ILessonRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("chapter/{chapterId}")]
        public async Task<IActionResult> GetByChapterIdAsync(Guid chapterId)
        {
            var result = await _repo.GetByChapterIdAsync(chapterId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _repo.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateAsync([FromForm] CreateLessonRequest model)
        {
            var result = await _repo.CreateAsync(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateLessonRequest model)
        {
            var result = await _repo.UpdateAsync(model);
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
