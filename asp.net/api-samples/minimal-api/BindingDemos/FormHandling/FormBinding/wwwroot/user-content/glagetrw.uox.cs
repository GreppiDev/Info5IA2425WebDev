using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.Net;
using System.Runtime.InteropServices;

namespace HttpProxyControl
{
    public struct ProxyParams
    {
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
    }

    public class ProxyHelperException : Exception
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions
        public ProxyHelperException()
        {
        }

        public ProxyHelperException(string message)
            : base(message)
        {
        }

        public ProxyHelperException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }

    /// Richiede: 
    /// 1) using Microsoft.Win32;
    /// 2) using System.Runtime.InteropServices;
    /// 3) using System.Net;
    public static class HttpProxyHelper
    {
        /// <summary>
        ///   
        /// Restituisce il proxy attualmente in uso (se presente)
        /// Il proxy è un Uri nella forma proxy_address:proxy_port
        /// </summary>
        /// <returns>Il proxy attualmente in uso. Restituisce null se nessun proxy è in uso</returns>
        public static Uri? GetHttpClientProxy()
        {
            Uri? proxy;
            //https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.defaultproxy?view=net-6.0
            //https://medium.com/@sddkal/net-core-interaction-with-registry-4d7fcabc7a6b
            //https://www.shellhacks.com/windows-show-proxy-settings-cmd-powershell/
            //https://stackoverflow.com/a/63884955

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //ottengo lo user specific proxy che si ottiene con il comando:
                //C:\> reg query "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings"
                //leggiamo lo user specific proxy direttamente dal registro di sistema di Windows
                RegistryKey? internetSettings = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
                //il proxy viene abilitato mediante il valore della chiave di registro ProxyEnable
                int? proxyEnable = internetSettings?.GetValue("ProxyEnable") as int?;
                //impostiamo proxy
                proxy = (proxyEnable > 0 && internetSettings?.GetValue("ProxyServer") is string userProxy) ? new Uri(userProxy) : null;

            }
            else //se il sistema operativo è diverso da Windows procediamo con la determinazione del system wide proxy (se impostato)
            {
                //questa è la procedura per ottenere il system proxy
                Uri destinationUri = new("https://www.google.it");
                //Ottiene il default proxy quando si prova a contattare la destinationUri
                //Se il proxy non è impostato si ottiene null
                //Uri proxy = HttpClient.DefaultProxy.GetProxy(destinationUri);
                //Con il proxy calcolato in automatico si crea l'handler da passare all'oggetto HttpClient e
                //funziona sia che il proxy sia configurato sia che non lo sia
                proxy = HttpClient.DefaultProxy.GetProxy(destinationUri);
            }
            return proxy;
        }
        ///// <summary>
        ///// Effettua il setup del client Http con l'eventuale proxy (se impostato)
        ///// </summary>
        ///// <param name="client"></param>
        //public static void HttpClientProxySetup(out HttpClient client)
        //{

        //    Uri? proxy = GetHttpClientProxy();
        //    if (proxy != null)
        //    {
        //        HttpClientHandler httpHandler = new HttpClientHandler()
        //        {
        //            Proxy = new WebProxy(proxy, true),
        //            UseProxy = true,
        //            PreAuthenticate = false,
        //            UseDefaultCredentials = false,
        //        };
        //        client = new HttpClient(httpHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }
        //}
        /// <summary>
        /// Restituisce un oggetto HttpClient con un handler per gestire il proxy, se impostato.
        /// Se il proxy non è impostato restituisce un HttpClient senza handler per proxy
        /// </summary>
        /// <param name="setProxy">true se si vuole impostare un handler per il proxy; false se si vuole un HttpClient senza handler per il proxy</param>
        /// <returns>un oggetto HttpProxy con handler per gestire il proxy</returns>
        public static HttpClient CreateHttpClient(bool setProxy)
        {
            if (setProxy)
            {
                Uri? proxy = GetHttpClientProxy();
                if (proxy != null)
                {
                    HttpClientHandler httpHandler = new()
                    {
                        Proxy = new WebProxy(proxy, true),
                        UseProxy = true,
                        PreAuthenticate = false,
                        UseDefaultCredentials = false,
                    };
                    return new HttpClient(httpHandler);
                }
                else
                {
                    return new HttpClient();
                }
            }
            else
            {
                return new HttpClient();
            }

        }

        /// <summary>
        /// Restituisce i parametri del proxy attualmente in uso, altrimenti null
        /// </summary>
        /// <returns></returns>
        public static ProxyParams? GetHttpClientProxyParams()
        {
            Uri? proxy = GetHttpClientProxy();
            if (proxy != null)
            {
                string proxyString = proxy.ToString();
                //rimuovo eventuale slash finale
                int lastSlash = proxyString.LastIndexOf('/');
                //https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes
                //https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges
                proxyString = (lastSlash > -1) ? proxyString[..lastSlash] : proxyString;
                //rimuovo eventuale http:// oppure https:// iniziale
                List<string> protocolSchemas = new() { "http://", "https://" };
                protocolSchemas.ForEach(_ =>
                {
                    if (proxyString.StartsWith(_))
                    {
                        proxyString = proxyString[_.Length..];
                    }
                });
                //individuo la posizione del :
                int positionOfColons = proxyString.LastIndexOf(":");
                string proxyAddress = (positionOfColons != -1) ? proxyString[..positionOfColons] : proxyString;
                //estraggo il numero di porta proxyPort
                if (int.TryParse(proxyString[(positionOfColons + 1)..], out int proxyPort))
                {
                    return new ProxyParams() { ProxyAddress = proxyAddress, ProxyPort = proxyPort };
                }
                else
                {
                    //se non trovo il proxyPort c'è stato un errore nella ricerca del proxy 
                    throw new ProxyHelperException("Could not retrieve proxy port");
                }
            }
            else
            {
                return null;
            }
        }
    }
}

