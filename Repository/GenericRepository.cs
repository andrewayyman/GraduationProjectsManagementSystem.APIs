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
        public Task AddAsync( T entity )
        {
            throw new NotImplementedException();
        }

        public void Delete( T entity )
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByIdAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Update( T entity )
        {
            throw new NotImplementedException();
        }
    }
}