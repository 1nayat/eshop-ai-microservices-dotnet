using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.VectorData;

namespace Catalog.Models;

public class ProductVector
{
    [VectorStoreKey]
    public ulong Id { get; set; }
    [VectorStoreData]
    public string Name { get; set; } = string.Empty;
    [VectorStoreData]
    public string Description { get; set; } = string.Empty;
    [VectorStoreData]
    public double Price { get; set; }
    [VectorStoreData]
    public string ImageUrl { get; set; } = string.Empty;

    [NotMapped]
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
