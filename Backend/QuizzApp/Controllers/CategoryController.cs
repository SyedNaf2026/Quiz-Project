using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Services;

namespace QuizzApp.Controllers
{
    // CategoryController handles quiz categories
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/category
        // View all categories (public - anyone can browse)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDTO>>.Ok(categories));
        }

        // POST api/category
        // Create a new category (QuizCreator only)
        [HttpPost]
       // [Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(ApiResponse<CategoryDTO>.Fail("Category name is required."));

            var result = await _categoryService.CreateCategoryAsync(dto);
            return Ok(ApiResponse<CategoryDTO>.Ok(result, "Category created successfully."));
        }
        // PUT api/category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDTO dto)
        {
            var (success, message, data) = await _categoryService.UpdateCategoryAsync(id, dto);
            if (!success) return BadRequest(ApiResponse<CategoryDTO>.Fail(message));
            return Ok(ApiResponse<CategoryDTO>.Ok(data!, message));
        }

        // DELETE api/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }
    }
}
