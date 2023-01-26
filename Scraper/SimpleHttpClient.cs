using System.Net;
using System.Net.Cache;
using System.Text;

/**
 * @author Valloon Present
 * @version 2022-11-19
 */
public static class SimpleHttpClient
{
    public static string HttpGet(string url, int timeout = 10)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
        var uri = new Uri(url);
        var response = client.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }

}
