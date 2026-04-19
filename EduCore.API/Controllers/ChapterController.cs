using EduCore.API.DTOs.Chapter;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterRepository _repo;
        public ChapterController(IChapterRepository repo)
        {
            _repo = repo;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _repo.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseIdAsync(Guid courseId)
        {
            var result = await _repo.GetByCourseIdAsync(courseId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(string title, int order, Guid courseId)
        {
            var result = await _repo.CreateAsync(title, order, courseId);
            return StatusCode(result.StatusCode, result);
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateAsync([FromBody] CreateChapterRequest model)
        //{
        //    var result = await _repo.CreateAsync(
        //        model.Title,
        //        model.Order,
        //        model.CourseId
        //    );

        //    return StatusCode(result.StatusCode, result);
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateChapterRequest model)
        {
            var result = await _repo.UpdateAsync(id, model.Title, model.Order, model.CourseId
            );

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
