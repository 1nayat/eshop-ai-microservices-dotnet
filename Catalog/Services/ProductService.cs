using Microsoft.EntityFrameworkCore;

namespace Catalog.Services;

public class ProductService(CatalogDbContext dbContext)
{
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await dbContext.Products.FindAsync(id);
    }

    public async Task CreateProductAsync(Product product)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(Product updatedProduct, Product inputProduct)
    {
        if (updatedProduct.Price != inputProduct.Price)
        {
            // Integration event (optional)
        }

        updatedProduct.Name = inputProduct.Name;
        updatedProduct.Description = inputProduct.Description;
        updatedProduct.ImageUrl = inputProduct.ImageUrl;
        updatedProduct.Price = inputProduct.Price;

        dbContext.Products.Update(updatedProduct);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Product deletedProduct)
    {
        dbContext.Products.Remove(deletedProduct);
        await dbContext.SaveChangesAsync();
    }

    // 🔥 FIXED NORMAL SEARCH
    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        // ✅ Handle empty search
        if (string.IsNullOrWhiteSpace(query))
        {
            return await dbContext.Products.ToListAsync();
        }

        query = query.Trim();

        // ✅ PostgreSQL optimized case-insensitive search
        return await dbContext.Products
            .Where(p =>
                EF.Functions.ILike(p.Name, $"%{query}%") ||
                EF.Functions.ILike(p.Description, $"%{query}%")
            )
            .ToListAsync();
    }
}