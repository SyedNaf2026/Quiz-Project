using System.Linq.Expressions;

namespace QuizzApp.Interfaces
{
    // Generic repository interface - works for any model type T
    // This single interface is used for ALL models (User, Quiz, Question, etc.)
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
