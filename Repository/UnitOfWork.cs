using Domain.Repository;
using Repository.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _Context;
        private readonly ConcurrentDictionary<string, object> _repositories;
        public UnitOfWork(ApplicationDbContext storeContext)
        {
            _Context = storeContext;
            _repositories = new();

        }
        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T).Name, _ => new GenericRepository<T>(_Context));
        }
    }
}
