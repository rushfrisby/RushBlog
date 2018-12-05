using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;

namespace RushBlog.DataAccess
{
	public class Database : IDatabase, IDisposable
	{
		private readonly DbContext _context;
		private readonly ConcurrentDictionary<Type, dynamic> _repos;

		public Database()
		{
			_context = new BlogContext();
			_repos = new ConcurrentDictionary<Type, dynamic>();
		}

		public IRepository<T> Repository<T>() where T : class
		{
			return _repos.GetOrAdd(typeof (T), new Repository<T>(_context));
		}

		public void Save()
		{
			_context.SaveChanges();
		}

		public bool CanConnect()
		{
			try
			{
				_context.Database.OpenConnection();
				_context.Database.CloseConnection();
			}
			catch
			{
				return false;
			}
			return true;
		}

		#region IDisposable implementation
		private bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
				_context.Dispose();

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
