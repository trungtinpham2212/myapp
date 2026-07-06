using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interfaces;
namespace Repositories.Implementation;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly myappContext _dbContext;

    public GenericRepository(myappContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TEntity> GetByIdAsync(object id, params Expression<Func<TEntity, object>>[] includes)
    {
        // Default: no tracking (for read-only operations)
        return GetByIdAsync(id, trackChanges: false, includes);
    }

    public async Task<TEntity> GetByIdAsync(object id, bool trackChanges, params Expression<Func<TEntity, object>>[] includes)
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        var key = entityType.FindPrimaryKey();

        if (key.Properties.Count != 1)
        {
            // Fallback: FindAsync for composite keys (always tracked)
            return await _dbContext.Set<TEntity>().FindAsync(id);
        }

        var keyName = key.Properties[0].Name;

        IQueryable<TEntity> query = _dbContext.Set<TEntity>();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Call(
            typeof(EF).GetMethod("Property").MakeGenericMethod(typeof(object)),
            parameter,
            Expression.Constant(keyName));
        var constant = Expression.Convert(Expression.Constant(id), typeof(object));
        var body = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        // Apply tracking based on parameter
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(lambda);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int? pageNumber = null,
        int? pageSize = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbContext.Set<TEntity>().AsQueryable();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
    {
        if (predicate == null)
        {
            return _dbContext.Set<TEntity>().CountAsync();
        }

        return _dbContext.Set<TEntity>().CountAsync(predicate);
    }

    public Task AddAsync(TEntity entity)
    {
        return _dbContext.Set<TEntity>().AddAsync(entity).AsTask();
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    public virtual async Task<(IReadOnlyList<TEntity> Items, int Total)>
GetPagedWithDetailsAsync(
    TEntity? filter,
    int pageNumber,
    int pageSize)
    {
        var query = _dbContext.Set<TEntity>().AsQueryable();

        var total = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

}


