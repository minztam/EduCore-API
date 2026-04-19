using EduCore.API.DTOs.Post;
using EduCore.API.Repositories.Implementations;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _repo;
        public PostController(IPostRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PostQuery query)
        {
            var result = await _repo.GetAllAsync(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _repo.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostRequest model)
        {
            var result = await _repo.CreateAsync(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] PostRequest model)
        {
            var result = await _repo.UpdateAsync(id, model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _repo.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id:guid}/publish")]
        public async Task<IActionResult> ChangePublishStatus(Guid id)
        {
            var result = await _repo.ChangePublishStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _repo.GetBySlugAsync(slug);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id:guid}/featured")]
        public async Task<IActionResult> ChangeFeaturedStatus(Guid id)
        {
            var result = await _repo.ChangeFeaturedStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int count = 5)
        {
            var result = await _repo.GetFeaturedPostsAsync(count);
            return StatusCode(result.StatusCode, result);
        }
    }
}
