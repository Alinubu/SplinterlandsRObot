using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SplinterlandsRObot.Net
{
    public class WebClient
    {
        HttpClient client;
        WebProxy? proxy = null;
        NetworkCredential? networkCredential = null;
        HttpClientHandler? httpClientHandler = null;

        public WebClient(string url, string? origin = "", string? referer = "", string? proxyUrl = null, string? proxyPort = null, string? proxyUser = null, string? proxyPassword = null)
        {
            httpClientHandler= new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli };
            if (proxyUrl is not null && proxyPort is not null)
            {
                proxy = new WebProxy()
                {
                    Address = new Uri($"http://{proxyUrl}:{proxyPort}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                };

                if (proxyUser is not null && proxyPassword is not null)
                {
                    networkCredential = new NetworkCredential(proxyUser, proxyPassword);
                    proxy.Credentials = networkCredential;
                }  
            }

            if (proxy is not null)
            {
                httpClientHandler.Proxy = proxy;
            }

            if (httpClientHandler is not null)
                client = new HttpClient(httpClientHandler, false);
            else client = new HttpClient();

            Uri uri = new Uri(url);
            client.BaseAddress = uri;

            if (origin != "")
                client.DefaultRequestHeaders.Add("origin", origin);
            if (referer != "")
                client.DefaultRequestHeaders.Add("referer", referer);
            client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.114 Safari/537.36");
            client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            client.Timeout = TimeSpan.FromSeconds(160);
        }

        public async Task<string> PostAsync(string postData, string path)
        {
            var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(client.BaseAddress + path, content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAsync(string path, AuthenticationHeaderValue? authHeader = null)
        {
            if (authHeader is not null) { client.DefaultRequestHeaders.Authorization = authHeader; }
            else { client.DefaultRequestHeaders.Remove("Authorization"); }

            HttpResponseMessage response = await client.GetAsync(client.BaseAddress + path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }
        }
    }
}
