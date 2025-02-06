using Microsoft.Extensions.Configuration;

namespace FilmAPI.Services;

public interface IRequestValidationService
{
    bool IsValidRequest(HttpContext context);
}

public class RequestValidationService : IRequestValidationService
{
    private readonly string[] _allowedDomains;

    public RequestValidationService(IConfiguration config)
    {
        _allowedDomains = config.GetSection("AllowedDomains").Get<string[]>()
            ?? throw new ArgumentNullException(nameof(config), "AllowedDomains configuration is missing");
    }

    public bool IsValidRequest(HttpContext context)
    {
        // Get Origin and Referer headers
        var origin = context.Request.Headers.Origin.ToString();
        var referer = context.Request.Headers.Referer.ToString();

        // If neither header is present, reject the request
        if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(referer))
        {
            return false;
        }

        // Local function to check if a URL starts with any allowed domain
        bool IsUrlFromAllowedDomain(string url) =>
            !string.IsNullOrEmpty(url) && _allowedDomains.Any(url.StartsWith);

        // Check both Origin and Referer headers
        return IsUrlFromAllowedDomain(origin) || IsUrlFromAllowedDomain(referer);
    }
}