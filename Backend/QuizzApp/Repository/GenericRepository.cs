using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.Interfaces;

namespace QuizzApp.Repository
{
    
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // The EF Core database context
        private readonly AppDbContext _context;

        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); 
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // Find rows matching a condition using a lambda expression
        // Example: FindAsync(u => u.Email == "test@test.com")
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}
