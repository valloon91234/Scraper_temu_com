using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal class Main
{
    public static void Start()
    {
        using var browserFetcher = new BrowserFetcher();
        var revisionInfo = browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Result;
        var browser = Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false
        }).Result;
        var page = browser.NewPageAsync().Result;
        //await page.GoToAsync("http://www.google.com");
        //await page.ScreenshotAsync(outputFile);
    }

}
