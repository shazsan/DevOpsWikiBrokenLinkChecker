using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker
{
    public class LinkCheckerBase
    {

        public static IWebDriver driver { get; set; }

        public IList<string> fnGetAllLinks()
        {

            List<string> LinksList = new List<string>();

            IList<IWebElement> LinkElements = driver.FindElements(By.TagName("a"));

            foreach (IWebElement item in LinkElements)
            {
                string URLHref = item.GetAttribute("href");
                if(URLHref!=null)
                {
                    LinksList.Add(URLHref);
                }                

            }

            return LinksList;

        }


        /// <summary>
        /// Method to launch browser
        /// </summary>
        /// <param name="browser"></param>
        public void LaunchBrowser(string browser)
        {
            //Checking browser

            switch (browser.ToLower().Split('+')[0])
            {
                case "firefox":
                    //      var ffOptions = new FirefoxOptions();
                    //      ffOptions.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";
                    //  ffOptions.LogLevel = FirefoxDriverLogLevel.Default;
                    //   ffOptions.Profile = new FirefoxProfile { AcceptUntrustedCertificates = true };
                    //  var service = FirefoxDriverService.CreateDefaultService();

                    FirefoxOptions FirefxOptions = new FirefoxOptions();

                    if (browser.Contains("headless"))
                    {
                        FirefxOptions.AddArguments("--headless");
                        FirefxOptions.AddArguments("disable-gpu");
                        FirefxOptions.AddArguments("window-size=1920,1080");
                    }


                    driver = new FirefoxDriver(FirefxOptions);

                    break;

                case "chrome":
                    ChromeOptions ChrOptions = new ChromeOptions();
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    service.HideCommandPromptWindow = true;

                    if (browser.Contains("headless"))
                    {
                        ChrOptions.AddArgument("--headless");
                        ChrOptions.AddArguments("disable-gpu");
                        ChrOptions.AddArguments("window-size=1920,1080");
                    }
                    driver = new ChromeDriver(service, ChrOptions);
                    break;

                default:

                    break;

            }
            
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        }

        public void CloseBrowser()
        {
            if (driver != null)
            {
                driver.Dispose();
            }
        }

        /// <summary>
        /// Method to navigate to url
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="browserType"></param>
        public void NavigateToURL(string URL)
        {
            try { 
                driver.Navigate().GoToUrl(URL);
            }
            catch
            {

            }


        }

        public HttpStatusCode GetHttpStatus(string url)
        {
            try
            {
                InitiateSSLTrust();
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

                ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };

                // 255 characters - lots of code!
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(
                        delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                        {
                            return true;
                        });

                HttpWebRequest webReq;
                webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.UseDefaultCredentials = true;
                webReq.UserAgent = "Link Checker";
                webReq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();

                return response.StatusCode;

            }

            catch (Exception e)
            {           
                return HttpStatusCode.NotImplemented;
            }

        }

        public static void InitiateSSLTrust()
        {
            try
            {
                //Change SSL checks so that all checks pass
                ServicePointManager.ServerCertificateValidationCallback =
                   new RemoteCertificateValidationCallback(
                        delegate
                        { return true; }
                    );
            }
            catch (Exception ex)
            {
              //  ActivityLog.InsertSyncActivity(ex);
            }
        }
    }
}
