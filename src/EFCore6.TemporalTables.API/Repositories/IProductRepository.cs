namespace EFCore6.TemporalTables.API.Repositories;

public interface IProductRepository
{
    Task<Product> GetAsync(Guid id);

    Task<Product> GetTemporalAsync(Guid id, DateTime time);

    Task<object> GetTemporalAsync(Guid id, DateTime from, DateTime to);

    Task<object> GetTemporalAsync(Guid id);

    Task<Product> RollbackTemporalDataAsync(Guid id, DateTime time);

    Task<Product> RestoreDeletedProductAsync(Guid id);

    Task<Product> CreateAsync(Product product);

    Task DeleteAsync(Guid id);

    Task<Product> UpdateAsync(Product product);
}
