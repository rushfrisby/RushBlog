using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RushBlog.DataAccess
{
	public class Repository<T> : IRepository<T> where T : class
	{

		private readonly DbContext _context;
		private readonly DbSet<T> _dbSet;

		public Repository(DbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_dbSet = _context.Set<T>();
		}

		public T Add(T entity)
		{
			_dbSet.Add(entity);
			return entity;
		}

		public void Delete(T entity)
		{
			if (_context.Entry(entity).State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}
			_dbSet.Remove(entity);
		}

		public T Delete<TId>(TId id)
		{
			var entity = Get(id);
			if (entity == null)
			{
				return null;
			}
			Delete(entity);
			return entity;
		}

		public void Update(T entity)
		{
			var entry = _context.Entry(entity);
			if (entry.State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}
			entry.State = EntityState.Modified;
		}

		public IQueryable<T> Where(Expression<Func<T, bool>> predicate, string[] includes = null)
		{
			var query = _dbSet.AsQueryable();

			if (includes != null && includes.Any())
			{
				foreach (var name in includes)
				{
					query.Include(name);
				}
			}

			return _dbSet.Where(predicate);
		}

		public IQueryable<T> All(string[] includes = null)
		{
			var query = _dbSet.AsQueryable();

			if (includes != null && includes.Any())
			{
				foreach (var name in includes)
				{
					query.Include(name);
				}
			}

			return _dbSet;
		}

		public T Get<TId>(TId id)
		{
			return _dbSet.Find(id);
		}
	}
}
