using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Repositories.Instances
{
    public class Repository<T>(TaskContext context) : IRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T> Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAll() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<T?> GetById(int id) => await _dbSet.FindAsync(id);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}
