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
                webRequest.Timeout = 15000;
                webRequest.ReadWriteTimeout = 15000;
                webRequest.Headers.Add("cookie", "region=211; language=en; currency=USD; api_uid=Cmy1OGPVZ02c+ABjA9ByAg==; timezone=Asia%2FYakutsk; goods=goods_tpfizn; webp=1; _nano_fp=XpE8n0UqX09YnqXxl9_sclH7H381jrq8DujRPX90; _bee=qCl6HDlDof6ys0cxWoXZcAGn6kqkOapO; njrpl=qCl6HDlDof6ys0cxWoXZcAGn6kqkOapO; dilx=60jWYP5aQ2U_1qVaT7K0h; _gcl_au=1.1.372831831.1674930049; gtm_logger_session=hnq4po8p4z9d2pwh2osu0; shipping_city=211; _ga=GA1.1.1441921199.1674930064; _device_tag=CgI2WRIIWG9MU3RkbnkaMBwf93B9+nX9qTmstsyu3xzaDXfJw5ZQ56riaxalhiGYk64ZYogVSw6JgFF0lRzKnjAC; AccessToken=BMEU2WAM3KKYLC6YRMK7O23OEFOFLHRRD4M6RA4PTJ37W57CGASQ0110d3c16c7d; user_uin=BD5TE4BKUCXNMLFPZK2SLALH3Q2T235SSK555BRW; PDDAccessToken=BMEU2WAM3KKYLC6YRMK7O23OEFOFLHRRD4M6RA4PTJ37W57CGASQ0110d3c16c7d; pdd_user_uin=BD5TE4BKUCXNMLFPZK2SLALH3Q2T235SSK555BRW; pdd_user_id=BD5TE4BKUCXNMLFPZK2SLALH3Q2T235SSK555BRW; _ga_R8YHFZCMMX=GS1.1.1674930063.1.1.1674930081.42.0.0");
                webRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
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
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        for (int i = 1800; i >= 0; i--)
                        {
                            Console.Write($"\rSleeping {TimeSpan.FromSeconds(i):mm\\:ss} ...");
                            Thread.Sleep(1000);
                        }
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

}
