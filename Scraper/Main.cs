using CsvHelper;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal class Main
{
    public static void Start()
    {
        var urlList = File.ReadAllText("link.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Logger.WriteLine($"{urlList.Count} URL addresses loaded.", ConsoleColor.DarkGray);
        var proxyList = File.ReadAllText("proxy4s.txt").Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

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
        var web = new HtmlWeb
        {
            PreRequest = delegate (HttpWebRequest webRequest)
            {
                webRequest.Timeout = 10000;
                webRequest.ReadWriteTimeout = 10000;
                return true;
            }
        };
        //var resultFilename = $"_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var resultFilename = $"_result.csv";
        int succeedCount = 0, soldCount = 0, failedCount = 0;
        int urlIndex = 0;
        int proxyIndex = 0;
        while (urlIndex < urlList.Count)
        {
            var url = urlList[urlIndex];
            Logger.WriteLine();
            Logger.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] \t {succeedCount} completed, {soldCount} sold out, {failedCount} failed, {urlList.Count} left");
            Logger.WriteLine($"{url}", ConsoleColor.Green);
            var proxy = proxyList[proxyIndex];
            Logger.WriteLine($"{proxyIndex + 1} / {proxyList.Count} \t {proxy}", ConsoleColor.DarkGray);
            var webProxy = new WebProxy
            {
                //Address = new Uri($"http://{proxy}"),
                Address = new Uri($"socks5://{proxy}"),
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
                        var value = doc.DocumentNode.QuerySelector(".curPrice-3HfXr span[data-type=price]").InnerText;
                        value = HtmlEntity.DeEntitize(value);
                        csvWriter.WriteField(value);
                        Logger.WriteLine($"price = \t {value}");
                    }
                    {
                        var value = "";
                        if (doc.DocumentNode.QuerySelectorAll(".prefixClsRtl-6Uznm>ol>li:nth-child(3)>a").Any())
                            value = doc.DocumentNode.QuerySelector(".prefixClsRtl-6Uznm>ol>li:nth-child(3)>a").InnerText;
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
                    if (doc.DocumentNode.QuerySelectorAll(".sku-3csHY .specSelector-4kzxj").Any())
                    {
                        var nodeList = doc.DocumentNode.QuerySelectorAll(".sku-3csHY .specSelector-4kzxj .skuItem-2MDh6").ToList();
                        var list = new List<string>();
                        foreach (var node in nodeList)
                        {
                            list.Add(HtmlEntity.DeEntitize(node.InnerText));
                        }
                        var value = "color : " + string.Join(" +++++ ", list);
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
                        var value = "size : " + string.Join(" +++++ ", list);
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
                }
                else
                {
                    File.WriteAllText("_failed.html", doc.DocumentNode.InnerHtml);
                    Logger.WriteLine("Failed", ConsoleColor.Red);
                    failedCount++;
                    if (failedCount % proxyList.Count == 0)
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            Logger.WriteLine($"Sleeping {60 - i} min...", ConsoleColor.DarkYellow);
                            Thread.Sleep(60000);
                        }
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

}
