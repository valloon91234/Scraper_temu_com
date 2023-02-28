using CsvHelper;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

internal class Main
{
    public static void Start()
    {
        var urlList = File.ReadAllText("link.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Logger.WriteLine($"{urlList.Count} URL addresses loaded.", ConsoleColor.DarkGray);
        var proxyList = File.ReadAllText("proxy.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var cookieList = File.ReadAllText("cookie.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        //var proxyList = new List<string>();
        //while (true)
        //{
        //    try
        //    {
        //        using var client = new HttpClient();
        //        //var uri = new Uri("https://proxylist.geonode.com/api/proxy-list?limit=500&page=1&sort_by=lastChecked&sort_type=desc&protocols=http");
        //        var uri = new Uri("https://proxylist.geonode.com/api/proxy-list?limit=500&page=1&sort_by=lastChecked&sort_type=desc&filterUpTime=90&speed=fast&protocols=socks5");
        //        var response = client.GetAsync(uri).Result;
        //        response.EnsureSuccessStatusCode();
        //        var responseText = response.Content.ReadAsStringAsync().Result;
        //        var jArray = (JArray)JObject.Parse(responseText)["data"]!;
        //        foreach (var el in jArray)
        //        {
        //            proxyList.Add($"{el["ip"]}:{el["port"]}");
        //        }
        //        break;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLine(ex.Message, ConsoleColor.Red);
        //    }
        //}

        Logger.WriteLine($"{proxyList.Count} proxies loaded.", ConsoleColor.DarkGray);

        //var resultFilename = $"_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var resultFilename = $"_result.csv";
        int succeedCount = 0, soldCount = 0, failedCount = 0;
        int urlIndex = 0;
        int proxyIndex = 0;
        int cookieIndex = 0;
        while (urlIndex < urlList.Count)
        {
            var web = new HtmlWeb
            {
                PreRequest = delegate (HttpWebRequest webRequest)
                {
                    webRequest.Timeout = 15000;
                    webRequest.ReadWriteTimeout = 15000;
                    webRequest.Headers.Add("cookie", cookieList[cookieIndex]);
                    webRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                    return true;
                }
            };
            var url = urlList[urlIndex];
            Logger.WriteLine();
            Logger.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] \t {succeedCount} completed, {soldCount} sold out, {failedCount} failed, {urlList.Count} left");
            Logger.WriteLine($"{url}", ConsoleColor.Green);
            var proxy = proxyList[proxyIndex];
            Logger.WriteLine($"{proxyIndex + 1} / {proxyList.Count} \t {proxy}", ConsoleColor.DarkGray);
            var webProxy = new WebProxy
            {
                Address = new Uri(proxy),
                BypassProxyOnLocal = true
            };
            //if (Debugger.IsAttached) webProxy = null;
            try
            {
                var doc = web.Load(url, "GET", webProxy, null);
                if (doc.DocumentNode.QuerySelectorAll(".titleWrap-NhsUf .goodsName-2rn4t").Any())
                {
                    using var writer = new StreamWriter(resultFilename, true);
                    using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    {
                        var value = doc.DocumentNode.QuerySelector(".titleWrap-NhsUf .goodsName-2rn4t").InnerText;
                        value = HtmlEntity.DeEntitize(value);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"title = \t {value}");
                    }
                    {
                        var value = doc.DocumentNode.QuerySelector("div[class^=curPrice] span[data-type=price]").InnerText;
                        value = HtmlEntity.DeEntitize(value);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"price = \t {value}");
                    }
                    {
                        var value = "";
                        if (doc.DocumentNode.QuerySelectorAll("nav[class^=prefixClsRtl]>ol>li:nth-child(3)>a").Any())
                            value = doc.DocumentNode.QuerySelector("nav[class^=prefixClsRtl]>ol>li:nth-child(3)>a").InnerText;
                        value = HtmlEntity.DeEntitize(value);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"category = \t {value}");
                    }
                    {
                        var nodeList = doc.DocumentNode.QuerySelectorAll(".thumbInner-2AOcl img").ToList();
                        var list = new List<string>();
                        foreach (var node in nodeList)
                        {
                            list.Add(node.Attributes["src"].Value);
                        }
                        var value = string.Join(" +++++ ", list);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"product_images = \t {list.Count}");
                    }
                    IEnumerable<HtmlNode> x = doc.DocumentNode.QuerySelectorAll(".sku-3csHY .colorHead-2aXpq");
                    if (x.Any() && x.First().InnerText.Trim().Length > 7)
                    {
                        var value = x.First().InnerText.Trim();
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Color/Size/Material = \t {value}");
                    }
                    else if (doc.DocumentNode.QuerySelectorAll(".spec-3QuQy .specSelector-4kzxj").Any())
                    {
                        var nodeList = doc.DocumentNode.QuerySelectorAll(".spec-3QuQy .specSelector-4kzxj .skuItem-2MDh6").ToList();
                        var list = new List<string>();
                        foreach (var node in nodeList)
                        {
                            list.Add(HtmlEntity.DeEntitize(node.InnerText));
                        }
                        //var value = "size : " + string.Join(" +++++ ", list);
                        var value = "size : " + list[0];
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Color/Size/Material = \t {value}");
                    }
                    else if (doc.DocumentNode.QuerySelectorAll(".sku-3csHY .spec-3cTw9").Any())
                    {
                        var value = doc.DocumentNode.QuerySelector(".sku-3csHY .spec-3cTw9").InnerText;
                        value = HtmlEntity.DeEntitize(value);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Color/Size/Material = \t {value}");
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                    {
                        var nodeList = doc.DocumentNode.QuerySelectorAll(".wrap-B_OB3 .item-1YBVO").ToList();
                        var list = new List<string>();
                        foreach (var node in nodeList)
                        {
                            var itemValue = HtmlEntity.DeEntitize(node.InnerText);
                            if (itemValue.EndsWith("Copy")) itemValue = itemValue[..^4];
                            list.Add(itemValue);
                        }
                        var value = string.Join(" +++++ ", list);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Description = \t {value}");
                    }
                    {
                        var nodeList = doc.DocumentNode.QuerySelectorAll(".imgList-1f5mc img").ToList();
                        var list = new List<string>();
                        foreach (var node in nodeList)
                        {
                            if (node.Attributes["src"] != null)
                                list.Add(node.Attributes["src"].Value);
                        }
                        var value = string.Join(" +++++ ", list);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"detail_images = \t {list.Count}");
                    }
                    {
                        var value = "0";
                        try
                        {
                            if (doc.DocumentNode.QuerySelectorAll(".top-3d092 .title-3ZiVV").Any())
                                value = doc.DocumentNode.QuerySelector(".top-3d092 .title-3ZiVV").InnerText.Split(' ')[0].Replace(",", "");
                        }
                        catch { }
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Reviews = \t {value}");
                    }
                    {
                        var value = "0";
                        try
                        {
                            if (doc.DocumentNode.QuerySelectorAll(".top-3d092 .score-3WU5r").Any())
                                value = doc.DocumentNode.QuerySelector(".top-3d092 .score-3WU5r .scoreText-RCmOr").InnerText.Trim();
                        }
                        catch { }
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"Score = \t {value}");
                    }
                    {
                        csvWriter.WriteField(url);
                    }
                    csvWriter.NextRecord();
                    //csvWriter.WriteRecord(new Row
                    //{
                    //    Title = title,
                    //    Price = price
                    //});
                    succeedCount++;
                    urlList.RemoveAt(urlIndex);
                    File.WriteAllLines("link.txt", urlList);
                }
                else if (doc.DocumentNode.QuerySelectorAll(".text-2woWW").Any())
                {
                    var value = doc.DocumentNode.QuerySelector(".text-2woWW").InnerText;
                    Logger.WriteLine(value, ConsoleColor.DarkYellow);
                    soldCount++;
                    urlList.RemoveAt(urlIndex);
                    File.WriteAllLines("link.txt", urlList);
                    File.AppendAllText("link_soldout.txt", url + Environment.NewLine);
                    //if (soldCount > 30)
                    //{
                    //    Logger.WriteLine($"soldCount = {soldCount}", ConsoleColor.Red);
                    //    break;
                    //}
                }
                else
                {
                    File.WriteAllText("_failed.html", doc.DocumentNode.InnerHtml);
                    Logger.WriteLine("Failed", ConsoleColor.Red);
                    failedCount++;
                    if (failedCount % proxyList.Count == 0)
                    {
                        cookieIndex = (cookieIndex + 1) % cookieList.Count;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //for (int i = 1800; i >= 0; i--)
                        //{
                        //    Console.Write($"\rSleeping {TimeSpan.FromSeconds(i):mm\\:ss} ...");
                        //    Thread.Sleep(1000);
                        //}
                        Console.Write($"Cookie updated.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Logger.WriteLine($"{message}", ConsoleColor.Red);
                if (proxyList.Count > 10 && (message.Contains("The operation has timed out") || message.Contains("target machine actively refused it")))
                {
                    proxyList.RemoveAt(proxyIndex);
                    proxyIndex--;
                }
            }

            proxyIndex++;
            if (proxyIndex >= proxyList.Count) proxyIndex = 0;
        }
    }

    public static void Start2()
    {
        var urlList = File.ReadAllText("link.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Logger.WriteLine($"{urlList.Count} URL addresses loaded.", ConsoleColor.DarkGray);
        var proxyList = File.ReadAllText("proxy.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Logger.WriteLine($"{proxyList.Count} proxies loaded.", ConsoleColor.DarkGray);
        var cookieList = File.ReadAllText("cookie.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Logger.WriteLine($"{cookieList.Count} cookies loaded.", ConsoleColor.DarkGray);

        int succeedCount = 0, soldCount = 0, failedCount = 0;
        int urlIndex = 0;
        int proxyIndex = 0;
        int cookieIndex = 0;
        int failedSum = 0;
        while (urlIndex < urlList.Count)
        {
            Logger.WriteLine();
            Logger.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] \t {succeedCount} completed, {soldCount} sold out, {failedCount} failed, {urlList.Count} left");
            var url = urlList[urlIndex];
            Logger.WriteLine($"{url}", ConsoleColor.Green);
            var proxy = proxyList[proxyIndex];
            Logger.WriteLine($"[proxy ({proxyIndex + 1} / {proxyList.Count})] \t {proxy}", ConsoleColor.DarkGray);

            var webProxy = new WebProxy
            {
                Address = new Uri(proxy),
                BypassProxyOnLocal = true,
                UseDefaultCredentials = false,
            };
            if (Debugger.IsAttached) webProxy = null;

            var cookie = cookieList[cookieIndex];
            Logger.WriteLine($"[cookie ({cookieIndex + 1} / {cookieList.Count})] \t {cookie[..40]}  ...  {cookie[^40..]}", ConsoleColor.DarkGray);

            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = webProxy,
                };

                using var client = new HttpClient(httpClientHandler, true);
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Add("cookie", cookie);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                var uri = new Uri(url);
                var response = client.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                var responseText = response.Content.ReadAsStringAsync().Result;

                if (responseText.Contains("<div class=\"text-2woWW clickable-1Bq8O\">"))
                {
                    var startIndex = responseText.IndexOf("<div class=\"text-2woWW clickable-1Bq8O\">") + "<div class=\"text-2woWW clickable-1Bq8O\">".Length;
                    var endIndex = responseText.IndexOf("<", startIndex);
                    var value = responseText[startIndex..endIndex];
                    Logger.WriteLine(value, ConsoleColor.Yellow);
                    soldCount++;
                    urlList.RemoveAt(urlIndex);
                    File.WriteAllLines("link.txt", urlList);
                    File.AppendAllText("link_soldout_real.txt", url + Environment.NewLine);
                    failedSum = 0;
                }
                else if (responseText.Contains("<div class=\"text-2woWW\">"))
                {
                    var startIndex = responseText.IndexOf("<div class=\"text-2woWW\">") + "<div class=\"text-2woWW\">".Length;
                    var endIndex = responseText.IndexOf("</div>", startIndex);
                    var value = responseText[startIndex..endIndex];
                    Logger.WriteLine(value, ConsoleColor.Red);
                    soldCount++;
                    urlList.RemoveAt(urlIndex);
                    File.WriteAllLines("link.txt", urlList);
                    File.AppendAllText("link_soldout.txt", url + Environment.NewLine);
                    failedSum++;
                }
                else if (responseText.Contains("window.rawData="))
                {
                    var startIndex = responseText.IndexOf("window.rawData=") + "window.rawData=".Length;
                    var endIndex = responseText.IndexOf("}};", startIndex);
                    if (endIndex > startIndex)
                    {
                        endIndex += "}};".Length - 1;
                        var resultJson = responseText[startIndex..endIndex];
                        File.AppendAllText("_result.txt", resultJson + Environment.NewLine);
                        succeedCount++;
                        urlList.RemoveAt(urlIndex);
                        File.WriteAllLines("link.txt", urlList);
                        Logger.WriteLine($"{resultJson[..50]}  ...  {resultJson[^50..]}", ConsoleColor.Gray);
                        File.WriteAllText("_www.html", responseText);
                    }
                    else
                    {
                        Logger.WriteLine("Json not ended.", ConsoleColor.Red);
                        var resultJson = responseText[startIndex..];
                        File.AppendAllText("_result.txt", resultJson + Environment.NewLine);
                        succeedCount++;
                        urlList.RemoveAt(urlIndex);
                        File.WriteAllLines("link.txt", urlList);
                        Logger.WriteLine($"{resultJson[..50]}  ...  {resultJson[^50..]}", ConsoleColor.Gray);
                    }
                    failedSum = 0;
                }
                else
                {
                    File.WriteAllText("_failed.html", responseText);
                    Logger.WriteLine("Failed", ConsoleColor.Red);
                    failedCount++;
                    failedSum++;
                }
                if (failedSum > 0 && failedSum % Math.Min(proxyList.Count, 2) == 0)
                {
                    cookieIndex = (cookieIndex + 1) % cookieList.Count;
                    //for (int i = 1800; i >= 0; i--)
                    //{
                    //    Console.Write($"\rSleeping {TimeSpan.FromSeconds(i):mm\\:ss} ...");
                    //    Thread.Sleep(1000);
                    //}
                    Logger.WriteLine($"Cookie updated -> {cookieIndex}", ConsoleColor.Red);
                    failedSum = 0;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Logger.WriteLine($"{message}", ConsoleColor.Red);
                if (proxyList.Count > 10 && (message.Contains("The operation has timed out") || message.Contains("target machine actively refused it")))
                {
                    proxyList.RemoveAt(proxyIndex);
                    proxyIndex--;
                }
            }

            proxyIndex++;
            if (proxyIndex >= proxyList.Count) proxyIndex = 0;
        }
    }

}
