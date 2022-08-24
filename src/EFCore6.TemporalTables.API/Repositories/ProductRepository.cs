using Microsoft.EntityFrameworkCore;

namespace EFCore6.TemporalTables.API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly TemporalDbContext _dbContext;

    public ProductRepository(TemporalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _dbContext.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product is null)
            return;

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Product> GetAsync(Guid id)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product> GetTemporalAsync(Guid id, DateTime time)
    {
        var snapshot = await _dbContext.Products
            .TemporalAsOf(time)
            .SingleOrDefaultAsync(product => product.Id == id);

        return snapshot;
    }

    public async Task<object> GetTemporalAsync(Guid id)
    {
        var snapshot = await _dbContext.Products
            .TemporalAll()
            .OrderBy(product => EF.Property<DateTime>(product, "Price"))
            .Where(product => product.Id == id)
            .Select(product =>
                new
                {
                    Product = product,
                    PeriodStart = EF.Property<DateTime>(product, "PeriodStart"),
                    PeriodEnd = EF.Property<DateTime>(product, "PeriodEnd")
                })
            .ToListAsync();

        return snapshot;
    }

    public async Task<object> GetTemporalAsync(Guid id, DateTime from, DateTime to)
    {
        var snapshot = await _dbContext.Products
            .TemporalFromTo(from, to)
            .OrderBy(product => EF.Property<DateTime>(product, "Price"))
            .Where(product => product.Id == id)
            .Select(product =>
                new
                {
                    Product = product,
                    PeriodStart = EF.Property<DateTime>(product, "PeriodStart"),
                    PeriodEnd = EF.Property<DateTime>(product, "PeriodEnd")
                })
            .ToListAsync();

        return snapshot;
    }

    public async Task<Product> RollbackTemporalDataAsync(Guid id, DateTime time)
    {
        var productChangedOn = _dbContext.Products
            .TemporalAsOf(time)
            .Where(product => product.Id == id)
            .Select(product => EF.Property<DateTime>(product, "PeriodEnd"))
            .Single();

        var rollbackedProduct = _dbContext.Products
            .TemporalAsOf(productChangedOn.AddMilliseconds(-1))
            .Single();

        _dbContext.Products.Update(rollbackedProduct);
        await _dbContext.SaveChangesAsync();

        return rollbackedProduct;
    }

    public async Task<Product> RestoreDeletedProductAsync(Guid id)
    {
        var productDeletedOn = _dbContext.Products
            .TemporalAll()
            .Where(product => product.Id == id)
            .OrderBy(product => EF.Property<DateTime>(product, "PeriodEnd"))
            .Select(product => EF.Property<DateTime>(product, "PeriodEnd"))
            .Last();

        var restoredProduct = _dbContext.Products
            .TemporalAsOf(productDeletedOn.AddMilliseconds(-1))
            .Single();

        _dbContext.Products.Add(restoredProduct);
        await _dbContext.SaveChangesAsync();

        return restoredProduct;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        var existingProduct = await _dbContext.Products.FindAsync(product.Id);

        existingProduct.Price = product.Price;

        _dbContext.Products.Update(existingProduct);

        await _dbContext.SaveChangesAsync();

        return existingProduct;
    }
}
