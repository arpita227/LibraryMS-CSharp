namespace LibraryMS.Interfaces
{
    /// <summary>
    /// Generic repository interface defining standard CRUD operations.
    /// Demonstrates the Repository Pattern and Interface-driven design.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}
