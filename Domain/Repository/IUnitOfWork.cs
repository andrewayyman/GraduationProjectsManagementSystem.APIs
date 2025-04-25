using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public interface IUnitOfWork
    {
        public Task<int> SaveChangesAsync();

        IGenericRepository<T> GetRepository<T>() where T : class;
    }
}
