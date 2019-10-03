using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.Tests
{
    [TestClass]
    public class DevOpsLinkChecker : LinkCheckerBase
    {
        public static ExtentReports extent;
        public static string RunFile;
        public static ExtentTest exTest;
        public TestContext TestContext { get; set; }

        public static string TestUrl = "https://wiki.hrblock.net/DevOps/WebHome";

        //Pass "" to check all links
        public static string TestUrlStartsWith = "https://wiki";

        //Mention Email Recipients sepearted by ";"
        public static string EmailRecipients = "sanah.shaz@hrblock.com";



        [TestInitialize]
        public void TestSetup()
        {
            extent = new ExtentReports();
            var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug", "");

            Directory.CreateDirectory(dir + "\\Test_Execution_Reports\\");

            RunFile = dir + "\\Test_Execution_Reports\\" + TestContext.TestName + "_" + DateTime.Now.ToLongDateString() + ".html";
            ExtentHtmlReporter htmlReporter = new ExtentHtmlReporter(RunFile);
            extent.AttachReporter(htmlReporter);
            exTest = extent.CreateTest(TestContext.TestName);

        }

        [TestMethod]
        public void DevOpsWikiCheckAllHomePageLinks()
        {
            LinkCheckerBase objLC = new LinkCheckerBase();
            objLC.LaunchBrowser("chrome");
            objLC.NavigateToURL(TestUrl);

            List<string> arrFoundLinks = new List<string>();
            var arrFoundLinksResults = new ConcurrentBag<string>();

            foreach (string LinkItem in fnGetAllLinks())
            {
                try
                {
                    if(TestUrlStartsWith != "")
                    {
                        if(LinkItem.StartsWith(TestUrlStartsWith))
                        { 
                            arrFoundLinks.Add(LinkItem);
                        }
                    }
                    else
                    {
                        arrFoundLinks.Add(LinkItem);
                    }


                }
                catch (Exception)
                {

                }
              
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 100
                //MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(arrFoundLinks, options, url =>
            {
                // EnsureUrlIsValid(url);

                var UrlStatus = GetHttpStatus(url).ToString();

                if (UrlStatus == "OK")
                {
                    arrFoundLinksResults.Add(url + " Status : " + UrlStatus + "--->>>> Link is valid");

                }
                else
                {
                    arrFoundLinksResults.Add(url + " Status : " + UrlStatus + "--->>>> Link is not valid");

                }

            }
            );

            foreach (string urlLine in arrFoundLinksResults)
            {
                try
                {

                    if (urlLine.Contains("Link is valid"))
                    {
                        exTest.Pass(urlLine);
                        TextWriter errorWriter = Console.Error;                      
                        System.Diagnostics.Debug.WriteLine(urlLine);
                    }
                    else
                    {
                        exTest.Fail(urlLine);
                        System.Diagnostics.Debug.WriteLine(urlLine);
                    }
                }
                catch (Exception e)
                {
                    
                }


            }

        }

        [TestCleanup]
        public void TestClean()
        {

            LinkCheckerBase objLC = new LinkCheckerBase();
            objLC.CloseBrowser();
            extent.Flush();
            TestContext.AddResultFile(RunFile);

            GenericHelper objGen = new GenericHelper();
            objGen.sendEMailThroughOUTLOOK(RunFile, EmailRecipients);


        }

    }
}
