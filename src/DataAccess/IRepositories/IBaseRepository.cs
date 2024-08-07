using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.IRepositories
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken);
        void UpdateAsync(T entity);
        void Delete(T entity);
        Task<T?> GetAsync(Guid id, CancellationToken cancellationToken);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> predicate = null);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetAllUntracked(CancellationToken cancellationToken);
    }
}
