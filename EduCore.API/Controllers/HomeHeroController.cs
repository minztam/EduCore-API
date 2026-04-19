using EduCore.API.DTOs.HomeHero;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeHeroController : ControllerBase
    {
        private readonly IHomeHeroRepository _repo;

        public HomeHeroController(IHomeHeroRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _repo.GetAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromForm] HomeHeroRequest req)
        {
            var result = await _repo.SaveAsync(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle()
        {
            var result = await _repo.ToggleAsync();
            return StatusCode(result.StatusCode, result);
        }
    }
}