using System;
using System.Linq;
using System.Linq.Expressions;

namespace RushBlog.DataAccess
{
    public interface IRepository<T> where T : class
    {
        T Add(T entity);

        void Delete(T entity);

        T Delete<TId>(TId id);

        void Update(T entity);

        IQueryable<T> Where(Expression<Func<T, bool>> predicate, string[] includes = null);

        IQueryable<T> All(string[] includes = null);

        T Get<TId>(TId id);
    }
}
