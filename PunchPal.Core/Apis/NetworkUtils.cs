using PunchPal.Core.Models;
using PunchPal.Core.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PunchPal.Core.Apis
{
    public class NetworkUtils
    {
        public static async Task<string> Get(string url)
        {
            var (text, _) = await Request(url, method: "GET");
            return text;
        }

        public static async Task<(string, string)> Request(string url, string data = "", string method = "POST", Dictionary<string, string> headers = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            data = data?.Trim() ?? string.Empty;
            var body = Encoding.UTF8.GetBytes(data);
            request.ContentLength = body.Length;
            var settings = SettingsModel.Load();
            if (settings.Network.RequestProxyType == ProxyType.System)
            {
                request.Proxy = WebRequest.DefaultWebProxy;
            }
            else if (settings.Network.RequestProxyType == ProxyType.Custom)
            {
                var proxy = new WebProxy(settings.Network.ProxyServerAddress, settings.Network.ProxyServerPort);
                if (settings.Network.IsProxyServerAuth)
                {
                    proxy.Credentials = new NetworkCredential(settings.Network.ProxyUserName, settings.Network.ProxyPassword);
                }
                else
                {
                    proxy.UseDefaultCredentials = true;
                }
                request.Proxy = proxy;
            }
            else
            {
                request.Proxy = null;
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    switch (header.Key.ToLower())
                    {
                        case "accept":
                            request.Accept = header.Value;
                            break;
                        case "content-type":
                            request.ContentType = header.Value;
                            break;
                        default:
                            request.Headers.Add(header.Key, header.Value);
                            break;
                    }
                }
            }

            try
            {

                if (body.Length > 0)
                {
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(body, 0, body.Length);
                    }
                }
                using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    var cookies = new List<string>();
                    foreach (Cookie item in response.Cookies)
                    {
                        cookies.Add($"{item.Name}={item.Value}");
                    }
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return (await reader.ReadToEndAsync(), string.Join("; ", cookies));
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                return (string.Empty, string.Empty);
            }

        }
    }
}
