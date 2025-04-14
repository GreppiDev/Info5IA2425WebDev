using System;

namespace EducationalGames.Utils;

public class HttpUtils
{
    // Funzione helper per determinare se la richiesta è da un browser o API
    public static bool IsHtmlRequest(HttpRequest request)
    {
        // Controlla se l'header Accept include HTML
        if (request.Headers.TryGetValue("Accept", out var acceptHeader))
        {
            return acceptHeader.ToString().Contains("text/html");
        }

        // Controlla l'header User-Agent per identificare i browser più comuni
        if (request.Headers.TryGetValue("User-Agent", out var userAgent))
        {
            string ua = userAgent.ToString().ToLower();
            if (ua.Contains("mozilla") || ua.Contains("chrome") || ua.Contains("safari") ||
                ua.Contains("edge") || ua.Contains("firefox") || ua.Contains("webkit"))
            {
                return true;
            }
        }
        return false;
    }

}
