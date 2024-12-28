namespace Task_Tracker_WebApp.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        public Task<T> Add(T entity);
        public Task<T?> GetById(int id);
        public Task<IEnumerable<T>> GetAll();
        public void Update(T entity);
        public void Delete(T entity);
    }
}
