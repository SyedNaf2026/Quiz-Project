using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    // CategoryService handles creating and listing quiz categories
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;

        public CategoryService(IGenericRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _categoryRepo.AddAsync(category);

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();

            // Manually map each category to a DTO
            return categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

    }
}
