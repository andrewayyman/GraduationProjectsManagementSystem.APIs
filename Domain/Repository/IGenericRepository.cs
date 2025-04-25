using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAllAsync();

        Task<T> GetByIdAsync( int id );

        Task<T> AddAsync( T entity );

        Task UpdateAsync( T entity );

        Task DeleteAsync( T entity );
    }
}