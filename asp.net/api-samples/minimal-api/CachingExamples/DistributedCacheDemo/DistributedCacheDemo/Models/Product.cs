using System;

namespace DistributedCacheDemo.Models;

// Modello Product
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime? LastUpdated { get; set; }
}

