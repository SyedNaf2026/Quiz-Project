using System.Collections.Generic;
using System.Threading.Tasks;
using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO dto);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
    }
}