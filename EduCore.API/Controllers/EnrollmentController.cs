using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentRepository _repo;
        public EnrollmentController(IEnrollmentRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollCourse(Guid userId, Guid courseId)
        {
            var result = await _repo.EnrollCourseAsync(userId, courseId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckEnrollment(Guid userId, Guid courseId)
        {
            var result = await _repo.CheckEnrollmentAsync(userId, courseId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses(Guid userId)
        {
            var result = await _repo.GetMyCoursesAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("progress")]
        public async Task<IActionResult> UpdateLearningProgress(Guid userId,Guid courseId,Guid lessonId)
        {
            var result = await _repo.UpdateLearningProgressAsync(userId,courseId,lessonId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
