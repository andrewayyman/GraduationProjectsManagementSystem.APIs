using Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository( ApplicationDbContext dbContext )
        {
            _dbContext = dbContext;
        }

        #region Crud

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync( int id )
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T> AddAsync( T entity )
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync( T entity )
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync( T entity )
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        #endregion Crud
    }
}