namespace RushBlog.DataAccess
{
    public interface IDatabase
    {
        IRepository<T> Repository<T>() where T : class;

        void Save();

        bool CanConnect();
    }
}