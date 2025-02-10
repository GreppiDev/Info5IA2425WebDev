using System;

namespace OpenStreetMaps.Services;

public class ArcGISLocationPlatformTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public ArcGISLocationPlatformTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetToken()
    {
        return _configuration["ArcGISLocationPlatform:Token"] ?? throw new InvalidOperationException("ArcGIS Location Platform token not configured");
    }
}
