using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetByIdAsync(object id, params Expression<Func<TEntity, object>>[] includes);
    
    Task<TEntity> GetByIdAsync(object id, bool trackChanges, params Expression<Func<TEntity, object>>[] includes);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int? pageNumber = null,
        int? pageSize = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);

    Task AddAsync(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<(IReadOnlyList<TEntity> Items, int Total)>
     GetPagedWithDetailsAsync(
         TEntity? filter,
         int pageNumber,
         int pageSize);
}


