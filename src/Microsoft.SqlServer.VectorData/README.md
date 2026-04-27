# Microsoft.SqlServer.VectorData

SQL Server and Azure SQL provider for [Microsoft.Extensions.VectorData](https://learn.microsoft.com/en-us/dotnet/ai/vector-stores/overview).

## Usage

```csharp
using Microsoft.Extensions.VectorData;
using Microsoft.SqlServer.VectorData;

// Define your record model
public sealed class BlogPost
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public string? Title { get; set; }

    [VectorStoreData]
    public string? Url { get; set; }

    [VectorStoreData]
    public string? Content { get; set; }

    [VectorStoreVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> ContentEmbedding { get; set; }
}

// Create the vector store and get a collection
var vectorStore = new SqlServerVectorStore(connectionString);
var collection = vectorStore.GetCollection<int, BlogPost>("BlogPosts");
await collection.EnsureCollectionExistsAsync();

// Upsert records
await collection.UpsertAsync(new BlogPost
{
    Id = 1,
    Title = "Vector search in Azure SQL",
    Content = "...",
    ContentEmbedding = embedding // ReadOnlyMemory<float> from your embedding provider
});

// Search
var results = await collection.SearchAsync(queryEmbedding, top: 5).ToListAsync();
```

## Documentation

- [Vector stores in .NET](https://learn.microsoft.com/en-us/dotnet/ai/vector-stores/overview)
