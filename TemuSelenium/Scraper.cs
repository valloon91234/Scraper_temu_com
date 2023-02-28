using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

class Scraper
{
    private const int DEFAULT_TIMEOUT_PAGELOAD = 180;

    static string? RunCmd(string cmd)
    {
        using Process p = new()
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c " + cmd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        try
        {
            p.Start();
            StreamReader sr = p.StandardOutput;
            string result = sr.ReadToEnd().Trim();
            sr.Close();
            return result;
        }
        catch { return null; }
    }


    static readonly Random Randomrandom = new();

    static string MakeRandomStr(int length)
    {
        string result = "";
        const string characters = "abcdefghijklmnopqrstuvwxyz0123456789";
        int charactersLength = characters.Length;
        int counter = 0;
        while (counter < length)
        {
            result += characters[Randomrandom.Next(charactersLength)];
            counter += 1;
        }
        return result;
    }
    static string MakeRandomEmail()
    {
        var length1 = Randomrandom.Next(6, 12);
        var length2 = Randomrandom.Next(3, 6);
        return MakeRandomStr(length1) + "@" + MakeRandomStr(length2) + ".com";
    }

    public static void Run()
    {
        var taskkillResult = RunCmd($"taskkill /f /im chromedriver.exe");
        Logger.WriteLine(taskkillResult);

        ChromeOptions options = new();
        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;
        //String username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        //if (username == @"DESKTOP-2KTBPSE\Valloon")
        //{
        //    var proxy = new Proxy();
        //    proxy.Kind = ProxyKind.Manual;
        //    proxy.IsAutoDetect = false;
        //    proxy.HttpProxy = proxy.SslProxy = "81.177.48.86:80";
        //    options.Proxy = proxy;
        //}

        //options.AddArgument("--start-maximized");
        //options.AddArgument("--auth-server-whitelist");
        //options.AddArguments("--disable-extensions");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--ignore-ssl-errors");
        options.AddArgument("--system-developer-mode");
        options.AddArgument("--no-first-run");
        options.SetLoggingPreference(LogType.Driver, LogLevel.All);
        //chromeOptions.AddArguments("--disk-cache-size=0");
        //options.AddArgument("--user-data-dir=" + m_chr_user_data_dir);
#if !DEBUG
            options.AddArguments("--headless");
            options.AddArguments("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");
            options.AddArguments("--disable-plugins-discovery");
            //options.AddArguments("--profile-directory=Default");
            //options.AddArguments("--no-sandbox");
            //options.AddArguments("--incognito");
            //options.AddArguments("--disable-gpu");
            //options.AddArguments("--no-first-run");
            //options.AddArguments("--ignore-certificate-errors");
            //options.AddArguments("--start-maximized");
            //options.AddArguments("disable-infobars");

            //options.AddAdditionalCapability("acceptInsecureCerts", true, true);
#endif
        var ChromeDriver = new ChromeDriver(chromeDriverService, options, TimeSpan.FromSeconds(DEFAULT_TIMEOUT_PAGELOAD));
        ChromeDriver.Manage().Window.Position = new Point(0, 0);
        ChromeDriver.Manage().Window.Size = new Size(1200, 900);
        ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        var JSE = (IJavaScriptExecutor)ChromeDriver;
        var Wait = new WebDriverWait(ChromeDriver, TimeSpan.FromSeconds(180));
        Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        string url = "https://www.temu.com/login.html?from=https%3A%2F%2Fwww.temu.com%2F&login_scene=2&refer_page_name=home&refer_page_id=10005_1676873404143_f9mbaf9b6x&refer_page_sn=10005&_x_sessn_id=llbgsvr9lr";
        Logger.WriteLine($"Opening:  {url}");
        ChromeDriver.Navigate().GoToUrl(url);

        var randomEmail = MakeRandomEmail();
        var emailInput = ChromeDriver.FindElement(By.Id("user-account"));
        emailInput.Clear();
        emailInput.SendKeys(randomEmail);

        var signinBtn = ChromeDriver.FindElement(By.Id("submit-button"));
        JSE.ExecuteScript("arguments[0].click();", signinBtn);

        var randomPassword = randomEmail + "!";
        var passwordInput = ChromeDriver.FindElement(By.Id("pwdInputInPddLoginDialog"));
        passwordInput.Clear();
        passwordInput.SendKeys(randomPassword);

        var registerBtn = ChromeDriver.FindElement(By.Id("submit-button"));
        JSE.ExecuteScript("arguments[0].click();", registerBtn);

    }


    //public void StartCvv(string[] array)
    //{
    //    Print();
    //    string result = null;
    //    foreach (string s in array)
    //    {
    //        string value = s.Trim();
    //        if (string.IsNullOrWhiteSpace(value)) continue;
    //        var submit = ChromeDriver.FindElement(By.Id("applyBtn"));
    //        try
    //        {
    //            var frame = ChromeDriver.SwitchTo().Frame("first-data-payment-field-cvv");
    //            var cvvEl = frame.FindElement(By.Id("cvv"));
    //            cvvEl.Clear();
    //            cvvEl.SendKeys(value);
    //            ChromeDriver.SwitchTo().DefaultContent();
    //            JSE.ExecuteScript("arguments[0].click();", submit);
    //            Thread.Sleep(1000);
    //            submit = ChromeDriver.FindElement(By.Id("applyBtn"));
    //            Wait.Until(d => !d.FindElement(By.Id("applyBtn")).GetAttribute("class").Contains("ajax-button-busy"));
    //            var captcha = ChromeDriver.FindElement(By.CssSelector("#main .g-recaptcha-wrapper"));
    //            var captchaDisplay = captcha.GetCssValue("display");
    //            if (!captchaDisplay.Equals("none", StringComparison.OrdinalIgnoreCase))
    //            {
    //                Print("Captcha appeared. Solve captcha, fix CVV list and press \"Find CVV\" again.");
    //                Print();
    //                return;
    //            }
    //            Print($"{value}");
    //            result = value;
    //        }
    //        catch (Exception)
    //        {
    //            Print();
    //            Print("- END -");
    //            break;
    //        }
    //    }
    //    if (result == null)
    //    {
    //        Print("Not found");
    //    }
    //    else
    //    {
    //        Print();
    //        Print($"Final Result:");
    //        Print($"{result}", ConsoleColor.Green);
    //        Print();
    //    }
    //}

}
