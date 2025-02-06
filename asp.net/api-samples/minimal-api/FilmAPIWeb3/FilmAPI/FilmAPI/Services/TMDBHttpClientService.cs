using System.Net.Http.Headers;
using FilmAPI.Utils;

namespace FilmAPI.Services;

public interface ITMDBHttpClientService
{
    Task<string> SendRequest(string endpoint, string query);
}

public sealed class TMDBHttpClientService : ITMDBHttpClientService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private bool _disposed;

    public TMDBHttpClientService(IConfiguration config)
    {
        _baseUrl = config["TMDB:BaseUrl"] ?? throw new ArgumentNullException("TMDB:BaseUrl configuration is missing");
        var bearerToken = config["TMDB:BearerToken"] ?? throw new ArgumentNullException("TMDB:BearerToken configuration is missing");

        // Create HttpClient with proxy support
        _httpClient = HttpProxyHelper.CreateHttpClient(setProxy: true);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    public async Task<string> SendRequest(string endpoint, string query)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new ArgumentException("The endpoint path is required", nameof(endpoint));
        }

        var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        if (!string.IsNullOrEmpty(query))
        {
            url += query;
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TMDBHttpClientService));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}