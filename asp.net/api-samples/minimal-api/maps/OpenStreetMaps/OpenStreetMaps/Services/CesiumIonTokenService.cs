using System;

namespace OpenStreetMaps.Services;

public class CesiumIonTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public CesiumIonTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetToken()
    {
        return _configuration["CesiumIon:Token"] ?? throw new InvalidOperationException("Cesium Ion token not configured");
    }
}
