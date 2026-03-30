using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Catalog.Services;

public class ProductAIService(
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<ulong, ProductVector> productVectorCollection,
    CatalogDbContext dbContext)
{
    // -----------------------------
    // SUPPORT CHAT
    // -----------------------------
    public async Task<string> SupportAsync(string userQuery, CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Select(p => new { p.Id, p.Name, p.Price })
            .ToListAsync(cancellationToken);

        string productCatalog = products.Count == 0
            ? "- (No products available)"
            : string.Join("\n", products.Select(p => $"- Id:{p.Id} Name:{p.Name} Price:(${p.Price})"));

        var systemPrompt = $"""
        You are a helpful assistant for an outdoor camping products store.
        Only answer camping-related queries. Be concise and slightly funny.

        Product Catalog:
        {productCatalog}
        """;

        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, userQuery)
        };

        var response = await chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);

        return response.Text ?? "No response.";
    }

    // -----------------------------
    // INIT EMBEDDINGS
    // -----------------------------
    private async Task InitEmbeddingsAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Initializing embeddings...");

        await productVectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        var products = await dbContext.Products.ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var productInfo = $"[{product.Name}] costs [{product.Price}] and is {product.Description}";

            
            var vector = await embeddingGenerator.GenerateVectorAsync(
                productInfo,
                options: null,
                cancellationToken: cancellationToken
            );

            var productVector = new ProductVector
            {
                Id = (ulong)product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                ImageUrl = product.ImageUrl,
                Vector = vector
            };

            await productVectorCollection.UpsertAsync(productVector, cancellationToken);
        }

        Console.WriteLine("Embeddings initialized.");
    }

    // -----------------------------
    // MAIN SEARCH METHOD
    // -----------------------------
    public async Task<IEnumerable<Product>> SearchProductsAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("Checking vector collection...");

            if (!await productVectorCollection.CollectionExistsAsync(cancellationToken))
            {
                await InitEmbeddingsAsync(cancellationToken);
            }

   
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            Console.WriteLine("Generating embedding...");

    
            var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(
                query,
                options: null,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Searching vector DB...");

            var results = productVectorCollection.SearchAsync(
                queryEmbedding,
                top: 3,
                cancellationToken: cts.Token
            );

            List<Product> products = new();

            await foreach (var result in results.WithCancellation(cts.Token))
            {
                products.Add(new Product
                {
                    Id = (int)result.Record.Id,
                    Name = result.Record.Name,
                    Description = result.Record.Description,
                    Price = (decimal)result.Record.Price,
                    ImageUrl = result.Record.ImageUrl
                });

                if (products.Count >= 3)
                    break;
            }

            if (products.Count == 0)
            {
                Console.WriteLine("AI returned no results, fallback to SQL...");
                return await FallbackSearch(query, cancellationToken);
            }

            return products;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI Search failed: {ex.Message}");

            return await FallbackSearch(query, cancellationToken);
        }
    }

    // -----------------------------
    // FALLBACK SEARCH (SQL)
    // -----------------------------
    private async Task<List<Product>> FallbackSearch(string query, CancellationToken cancellationToken)
    {
        Console.WriteLine("Executing fallback SQL search...");

        return await dbContext.Products
            .Where(p => p.Name.Contains(query))
            .ToListAsync(cancellationToken);
    }
}