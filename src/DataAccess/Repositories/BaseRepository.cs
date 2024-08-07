using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly ApiDbContext _context;
        protected readonly DbSet<T> _entities;
        public BaseRepository(ApiDbContext apiDbContext)
        {
            _context = apiDbContext;
            _entities = apiDbContext.Set<T>();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
        {
            await _entities.AddAsync(entity, cancellationToken);
            return entity;
        }

        public void Delete(T entity)
        {
            if (entity is BaseEntity)
            {
                var prop = entity.GetType().GetProperty("Status");
                prop.SetValue(entity, Status.Disabled);
                _context.Entry(entity).State = EntityState.Deleted;

            }
            else
                _entities.Remove(entity);

        }

        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken,Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _entities;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(id, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void UpdateAsync(T entity)
        {
            _entities.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async virtual Task<IEnumerable<T>> GetAllUntracked(CancellationToken cancellationToken)
        {
            return await _entities.AsNoTracking().ToListAsync(cancellationToken);
        }

    }
}
