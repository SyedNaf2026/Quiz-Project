using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly AppDbContext _context;

        public CategoryService(IGenericRepository<Category> categoryRepo, AppDbContext context)
        {
            _categoryRepo = categoryRepo;
            _context = context;
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

            // Check if any quizzes are using this category
            var inUse = await _context.Quizzes.AnyAsync(q => q.CategoryId == id);
            if (inUse) return (false, "Cannot delete — this category is used by one or more quizzes.");

            await _categoryRepo.DeleteAsync(category);
            return (true, "Category deleted.");
        }
    }
}
