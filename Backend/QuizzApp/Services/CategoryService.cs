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
            return categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<(bool Success, string Message, CategoryDTO? Data)> UpdateCategoryAsync(int id, CreateCategoryDTO dto)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return (false, "Category not found.", null);

            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Category name is required.", null);

            category.Name = dto.Name;
            category.Description = dto.Description;
            await _categoryRepo.UpdateAsync(category);

            return (true, "Category updated.", new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return (false, "Category not found.");

            await _categoryRepo.DeleteAsync(category);
            return (true, "Category deleted.");
        }
    }
}
