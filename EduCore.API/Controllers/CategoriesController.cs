using EduCore.API.DTOs.Categories;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _repo;

        public CategoriesController(ICategoriesRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _repo.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryRequest req)
        {
            var result = await _repo.CreateAsync(req.Name, req.Slug,req.Type,req.ParentId,req.SortOrder);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCategoryRequest req)
        {
            var result = await _repo.UpdateAsync(id,req.Name,req.Slug,req.Type, req.ParentId,req.SortOrder );

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatusAsync(Guid id)
        {
            var result = await _repo.ToggleStatusAsync(id);
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