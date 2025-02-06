using System;

namespace AzureMaps.Services;

public class AzureMapsKeyService : IMapKeyService
{
    private readonly IConfiguration _configuration;

    public AzureMapsKeyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetMapKey()
    {
        return _configuration["AzureMaps:SharedKey"] ?? throw new InvalidOperationException("Azure Maps API key not configured");
    }
}
