using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories
{
    public class GenericRepository<T> where T : class
    {
        // Generic Repository
        private readonly OCMSDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(OCMSDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
    }
}
