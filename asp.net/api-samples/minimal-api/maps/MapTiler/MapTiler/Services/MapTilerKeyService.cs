namespace MapTiler.Services;



public class MapTilerKeyService : IMapKeyService
{
    private readonly IConfiguration _configuration;

    public MapTilerKeyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetMapTilerKey()
    {
        return _configuration["MapTiler:ApiKey"] ?? throw new InvalidOperationException("MapTiler API key not configured");
    }
}