using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Core.Constants;

namespace Service.Helper
{
    public class ServiceHelper
    {
        public const int MAX_FILES = 1;
        private const int MAX_RETRIES = 5;
        private const int MAX_FILE_RETRIES = 5;
        private const String PAGEVIEW_NAME_TEMPLATE = "pageviews-{0}{1}{2}-{3}0000.gz";
        private const String PAGEVIEW_URL_TEMPLATE = "{0}/{0}-{1}/";
        private static List<DateTime> lastFiles = new();
        private static bool printDownloadHeader = true;
        static public List<string> GetFilenames()
        {
            List<string> filenames = new List<string>();
            for (int index = 0; index < lastFiles.Count; index++)
            {
                filenames.Add(FormatFileName(lastFiles[index]));
            }
            return filenames;
        }

        static public List<string> GetUrls()
        {
            List<string> urls = new List<string>();
            for (int index = 0; index < lastFiles.Count; index++)
            {
                urls.Add(FormatFileUrl(lastFiles[index]));
            }
            return urls;
        }

        public static void FindLastFiles()
        {
            DateTime timestamp = DateTime.UtcNow;
            timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
            int totalRetries = 0;
            int fileRetries = 0;
            WriteHeader();
            while (lastFiles.Count < MAX_FILES && totalRetries < MAX_RETRIES)
            {
                string filename = FormatFileName(timestamp);
                UriBuilder uriBuilder = new UriBuilder(String.Concat(ApiUrl.BaseUrl, FormatFileUrl(timestamp), filename));
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriBuilder.Uri);
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        lastFiles.Add(timestamp);
                        timestamp = timestamp.AddHours(-1);
                        fileRetries = 0;
                        ReportFoundFiles();
                        DelayRequest(true);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        timestamp = timestamp.AddHours(-1);
                        fileRetries = 0;
                        totalRetries++;
                        Console.WriteLine(@"File was unavilable, skipping file: {0}", filename);
                        Console.WriteLine(@"Error: {0}", ex.Message);
                        DelayRequest(true);
                    }
                    else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        if (fileRetries == 0)
                        {
                            Console.WriteLine(@"Service was unavilable, slowing requests for file: {0}", filename);
                            Console.WriteLine(@"Error: {0}", ex.Message);
                            Console.WriteLine(@"Retrying: ");
                        }
                        if (fileRetries < MAX_FILE_RETRIES)
                        {
                            fileRetries++;
                            Console.Write(@"Attempt {0}", fileRetries);
                            DelayRequest(false, true, fileRetries);
                            Console.Write(@" | ");
                        }
                        else
                        {
                            timestamp = timestamp.AddHours(-1);
                            fileRetries = 0;
                            totalRetries++;
                            Console.WriteLine();
                            DelayRequest(true);
                        }
                    }
                }
            }
        }

        static private DateTime GetCurrentTime()
        {
            DateTime timestamp = DateTime.UtcNow.AddHours(-1);
            return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
        }

        static private string FormatFileName(DateTime timestamp)
        {
            string hourStr = '0' + timestamp.Hour.ToString();
            return String.Format(
                PAGEVIEW_NAME_TEMPLATE,
                timestamp.Year.ToString(),
                timestamp.Month.ToString(),
                timestamp.Day.ToString(),
                hourStr[^2..]
                );
        }

        static private string FormatFileUrl(DateTime timestamp)
        {
            return String.Format(
                    PAGEVIEW_URL_TEMPLATE,
                    timestamp.Year.ToString(),
                    timestamp.Month.ToString()
                    );
        }

        static private void DelayRequest(bool includeMessage, bool includeDots = false, int delayMultiplier = 1)
        {
            if (includeMessage) Console.Write("Delaying requests to avoid service overload");
            for (int i = 0; i < 5; i++)
            {
                if (includeMessage || includeDots) Console.Write(".");
                Thread.Sleep(2000 * delayMultiplier);
            }
            if (includeMessage) Console.WriteLine();
        }

        static private void ReportFoundFiles()
        {
            WriteHeader();
            for (int i = 0; i < lastFiles.Count; i++)
            {
                Console.WriteLine(@"Dump {0} - File name: {1}", i + 1, FormatFileName(lastFiles[i]));
            }
        }

        static private void WriteHeader()
        {
            Console.Clear();
            Console.WriteLine(@"{0}{1}", new string(' ', 5), "WIKIMEDIA COUNT TOOL");
            Console.WriteLine(new string('=', 30));
        }

        static public void WriteDownloadHeader()
        {
            if (printDownloadHeader)
            {
                Console.WriteLine(new string('=', 30));
                Console.WriteLine(@"{0}{1}", new string(' ', 5), "DOWNLOADING FILES");
                Console.WriteLine(new string('=', 30));
                printDownloadHeader = false;
            }
        }
    }
}
