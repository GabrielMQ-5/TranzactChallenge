using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Core.Constants;
using Core.Entities;

namespace Service.Helper
{
    public class ServiceHelper
    {
        private const String PAGEVIEW_NAME_TEMPLATE = "pageviews-{0}{1}{2}-{3}0000.gz";
        private const String PAGEVIEW_URL_TEMPLATE = "{0}/{0}-{1}/";
        private const int RESET_SCREEN_DELAY = 20;
        private static String CONSOLE_MESSAGE = "";
        private static bool requestsDelayed = false;
        private static int clearScreen = RESET_SCREEN_DELAY;
        public enum HeaderType : ushort { downloadHeader = 0, resultHeader = 1 };

        public static List<string> exceptionsEncountered = new();

        static public List<string> GetFilenames(List<DateTime> lastFiles)
        {
            List<string> filenames = new List<string>();
            try
            {
                for (int index = 0; index < lastFiles.Count; index++)
                {
                    filenames.Add(FormatFileName(lastFiles[index]));
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
            return filenames;
        }

        static public List<string> GetUrls(List<DateTime> lastFiles)
        {
            List<string> urls = new List<string>();
            try
            {
                for (int index = 0; index < lastFiles.Count; index++)
                {
                    urls.Add(FormatFileUrl(lastFiles[index]));
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
            return urls;
        }

        static public DateTime GetCurrentTime()
        {
            try
            {
                DateTime timestamp = DateTime.UtcNow.AddHours(-1);
                return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
                return new DateTime();
            }
        }

        static public string FormatFileName(DateTime timestamp)
        {
            try
            {
                return String.Format(
                PAGEVIEW_NAME_TEMPLATE,
                timestamp.Year.ToString(),
                ('0' + timestamp.Month.ToString())[^2..],
                ('0' + timestamp.Day.ToString())[^2..],
                ('0' + timestamp.Hour.ToString())[^2..]
                );
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
                return "";
            }
        }

        static public string FormatFileUrl(DateTime timestamp)
        {
            try
            {
                return String.Format(
                    PAGEVIEW_URL_TEMPLATE,
                    timestamp.Year.ToString(),
                    ('0' + timestamp.Month.ToString())[^2..]
                    );
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
                return "";
            }
        }

        static public void DelayRequest(int delayOffset = 0, int delayMultiplier = 3)
        {
            try
            {
                requestsDelayed = true;
                CONSOLE_MESSAGE = "Delaying requests to avoid service overload";
                int multiplier = Math.Max(delayMultiplier, 0);
                for (int i = 0; i < 5; i++)
                {
                    CONSOLE_MESSAGE += ".";
                    Thread.Sleep((1250 * multiplier) * (1 + delayOffset));
                }
                CONSOLE_MESSAGE = "";
                requestsDelayed = false;
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void ResetScreen()
        {
            try
            {
                clearScreen = 0;
                Console.Clear();
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void WriteMainHeader()
        {
            try
            {
                string header = "WIKIMEDIA COUNT TOOL";
                int offset = Console.WindowWidth / 2 - header.Length / 2;
                Console.SetCursorPosition(0, 0);
                Console.Write(new string('=', Console.WindowWidth));
                Console.Write(@"{0}{1}{2}",
                    new string(' ', offset),
                    header,
                    new string(' ', Console.WindowWidth - (offset + header.Length)));
                Console.Write(new string('=', Console.WindowWidth));
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void ReportFoundFiles(List<DateTime> lastFiles, List<ProgressBar> progressBars, List<FileDownloadResult> fileResults, bool resetScreen = false)
        {
            try
            {
                if (clearScreen == 0 || resetScreen) { Console.Clear(); clearScreen = RESET_SCREEN_DELAY; }
                else clearScreen--;
                WriteMainHeader();
                for (int i = 0; i < lastFiles.Count; i++)
                {
                    Console.WriteLine(@"Dump {0} - File name: {1}", i + 1, FormatFileName(lastFiles[i]));
                }
                if (progressBars.Count > 0) WriteReportHeader(HeaderType.downloadHeader);
                for (int i = 0; i < progressBars.Count; i++)
                {
                    Console.WriteLine(progressBars[i].GetCurrentText());
                }
                if (fileResults.Count > 0) WriteReportHeader(HeaderType.resultHeader);
                for (int i = 0; i < fileResults.Count; i++)
                {
                    string resultString = String.Format(@"File {0} - {1}", fileResults[i].GetFileName(), fileResults[i].GetResult());
                    if (fileResults[i].GetError() != null) resultString += String.Format(@": {0}{1}", fileResults[i].GetError(),
                        fileResults[i].GetRetries() > 0 ? String.Format(@" | Retries: {0}", fileResults[i].GetRetries()) : "");
                    Console.WriteLine(resultString);
                }
                if (requestsDelayed) Console.WriteLine(CONSOLE_MESSAGE);
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void WriteReportHeader(HeaderType downloadHeaderType)
        {
            try
            {
                string header = downloadHeaderType == HeaderType.resultHeader ? "FILE DOWNLOAD RESULTS" :
                downloadHeaderType == HeaderType.downloadHeader ? "DOWNLOAD PROGRESS" :
                "";
                int offset = Console.WindowWidth / 2 - header.Length / 2;
                if (downloadHeaderType == HeaderType.downloadHeader) Console.SetCursorPosition(0, 3 + Constants.MAX_FILES);
                else Console.SetCursorPosition(0, 6 + Constants.MAX_FILES * 2);
                Console.Write(new string('=', Console.WindowWidth));
                Console.Write(@"{0}{1}{2}",
                    new string(' ', offset),
                    header,
                    new string(' ', Console.WindowWidth - (offset + header.Length)));
                Console.Write(new string('=', Console.WindowWidth));
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void ReportDecompressedFiles(List<DownloadedFile> downloadedFiles)
        {
            try
            {
                if (clearScreen == 0) { Console.Clear(); clearScreen = RESET_SCREEN_DELAY; }
                else clearScreen--;
                List<string> decompressedFiles = new();
                WriteMainHeader();
                for (int i = 0; i < downloadedFiles.Count; i++)
                {
                    string dFile = String.Format(@"Dump {0} - File name: {1} | {2}%", i + 1, downloadedFiles[i].GetFileName(), downloadedFiles[i].GetPercentage());
                    Console.WriteLine(@"{0}{1}", dFile, new string(' ', Console.WindowWidth - (dFile.Length)));
                    if (downloadedFiles[i].GetDecompressedFileName() != null) decompressedFiles.Add(downloadedFiles[i].GetDecompressedFileName());
                }
                if (decompressedFiles.Count > 0) WriteDecompressHeader();
                for (int i = 0; i < decompressedFiles.Count; i++)
                {
                    Console.WriteLine(@"Unzipped file: {0}", decompressedFiles[i]);
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void WriteDecompressHeader()
        {
            try
            {
                string header = "FILES DECOMPRESSED";
                int offset = Console.WindowWidth / 2 - header.Length / 2;
                Console.SetCursorPosition(0, 3 + Constants.MAX_FILES);
                Console.Write(new string('=', Console.WindowWidth));
                Console.Write(@"{0}{1}{2}",
                    new string(' ', offset),
                    header,
                    new string(' ', Console.WindowWidth - (offset + header.Length)));
                Console.Write(new string('=', Console.WindowWidth));
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void ReportCondensedEntries()
        {
            try
            {
                Console.Clear();
                WriteMainHeader();
                CONSOLE_MESSAGE = "Condensing results";
                for (int i = 0; i < 5; i++)
                {
                    Console.Write(CONSOLE_MESSAGE);
                    Thread.Sleep(250);
                    CONSOLE_MESSAGE = ".";
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void PrintEntries(List<ViewCountEntry> viewCountEntries, int maxDomainLength, int maxPageTitleLength, int maxViewCountLength)
        {
            try
            {
                Console.Clear();
                WriteMainHeader();

                string domain = String.Format(@"{0}{1}", "DOMAIN", new string(' ', Math.Max(maxDomainLength - "DOMAIN".Length, 0)));
                string pageTitle = String.Format(@"{0}{1}", "PAGE TITLE", new string(' ', Math.Max(maxPageTitleLength - "PAGE TITLE".Length, 0)));
                string viewCount = String.Format(@"{0}{1}", "VIEW COUNT", new string(' ', Math.Max(maxViewCountLength - "VIEW COUNT".Length, 0)));
                string tableTitle = String.Format(@"{0}|{1}|{2}", domain, pageTitle, viewCount);
                Console.WriteLine(tableTitle);
                Console.WriteLine(new string('=', tableTitle.Length));

                foreach (var entry in viewCountEntries)
                {
                    domain = String.Format(@"{0}{1}", entry.GetDomain(), new string(' ', Math.Max(maxDomainLength - entry.GetDomain().Length, 0)));
                    pageTitle = String.Format(@"{0}{1}", entry.GetPageTitle(), new string(' ', Math.Max(maxPageTitleLength - entry.GetPageTitle().Length, 0)));
                    viewCount = String.Format(@"{0}{1}", entry.GetViewCount(), new string(' ', Math.Max(maxViewCountLength - entry.GetViewCount().ToString().Length, 0)));
                    Console.WriteLine(@"{0}|{1}|{2}", domain, pageTitle, viewCount);
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
    }
}
