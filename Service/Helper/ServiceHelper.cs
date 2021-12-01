using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Core.Constants;
using Core.Entities;

namespace Service.Helper
{
    public static class ServiceHelper
    {
        private const String PAGEVIEW_NAME_TEMPLATE = "pageviews-{0}{1}{2}-{3}0000.gz";
        private const String PAGEVIEW_URL_TEMPLATE = "{0}/{0}-{1}/";
        private const int RESET_SCREEN_DELAY = 20;
        private static String CONSOLE_MESSAGE = "";
        private static bool requestsDelayed = false;
        private static int clearScreen = RESET_SCREEN_DELAY;
        public enum HeaderType : ushort { mainHeader = 0, downloadHeader = 1, resultHeader = 2, decompressedHeader = 3 };

        public static List<string> exceptionsEncountered = new();
        public static bool unitTest = false;

        #region UTIL
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
                if (!unitTest) Console.Clear();
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region STRING FORMAT
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
        #endregion

        #region HEADER
        static public void WriteHeader(HeaderType headerType)
        {
            try
            {
                string header = headerType == HeaderType.mainHeader ? "WIKIMEDIA COUNT TOOL" :
                    headerType == HeaderType.resultHeader ? "FILE DOWNLOAD RESULTS" :
                headerType == HeaderType.downloadHeader ? "DOWNLOAD PROGRESS" :
                headerType == HeaderType.decompressedHeader ? "FILES DECOMPRESSED" :
                "";
                int consoleWidth = (!unitTest) ? Console.WindowWidth : header.Length;
                int offset = consoleWidth / 2 - header.Length / 2;

                string breakerLine = new string('=', consoleWidth);
                string titleLine = String.Format(@"{0}{1}{2}",
                    new string(' ', offset),
                    header,
                    new string(' ', consoleWidth - (offset + header.Length)));

                int cursorPosition = 0;
                cursorPosition =
                    (headerType == HeaderType.downloadHeader) ? (3 + Constants.MAX_FILES) :
                    (headerType == HeaderType.resultHeader) ? (6 + Constants.MAX_FILES * 2) :
                    (headerType == HeaderType.decompressedHeader) ? (3 + Constants.MAX_FILES) :
                    0;

                if (!unitTest)
                {
                    Console.SetCursorPosition(0, cursorPosition);
                    Console.Write(breakerLine);
                    Console.Write(titleLine);
                    Console.Write(breakerLine);
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region FOUND FILES
        static public void ReportFoundFiles(List<DateTime> lastFiles, List<ProgressBar> progressBars, List<FileDownloadResult> fileResults, bool resetScreen = false)
        {
            try
            {
                if (clearScreen == 0 || resetScreen)
                {
                    if (!unitTest) Console.Clear();
                    clearScreen = RESET_SCREEN_DELAY;
                }
                else clearScreen--;
                WriteHeader(HeaderType.mainHeader);
                for (int i = 0; i < lastFiles.Count; i++)
                {
                    string fileInfo = String.Format(@"Dump {0} - File name: {1}", i + 1, FormatFileName(lastFiles[i]));
                    if (!unitTest) Console.WriteLine(fileInfo);
                }
                if (progressBars.Count > 0) WriteHeader(HeaderType.downloadHeader);
                for (int i = 0; i < progressBars.Count; i++)
                {
                    if (!unitTest) Console.WriteLine(progressBars[i].GetCurrentText());
                }
                if (fileResults.Count > 0) WriteHeader(HeaderType.resultHeader);
                for (int i = 0; i < fileResults.Count; i++)
                {
                    string resultString = String.Format(@"File {0} - {1}", fileResults[i].GetFileName(), fileResults[i].GetResult());
                    if (fileResults[i].GetError() != null) resultString += String.Format(@": {0}{1}", fileResults[i].GetError(),
                        fileResults[i].GetRetries() > 0 ? String.Format(@" | Retries: {0}", fileResults[i].GetRetries()) : "");
                    if (!unitTest) Console.WriteLine(resultString);
                }
                if (requestsDelayed && !unitTest) Console.WriteLine(CONSOLE_MESSAGE);
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region DECOMPRESSED FILES
        static public void ReportDecompressedFiles(List<DownloadedFile> downloadedFiles)
        {
            try
            {
                if (clearScreen == 0)
                {
                    if (!unitTest) Console.Clear();
                    clearScreen = RESET_SCREEN_DELAY;
                }
                else clearScreen--;
                List<string> decompressedFiles = new();
                WriteHeader(HeaderType.mainHeader);
                for (int i = 0; i < downloadedFiles.Count; i++)
                {
                    string dFile = String.Format(@"Dump {0} - File name: {1} | {2}%", i + 1, downloadedFiles[i].GetFileName(), downloadedFiles[i].GetPercentage());
                    int consoleWidth = (!unitTest) ? Console.WindowWidth : dFile.Length;
                    string fileInfo = String.Format(@"{0}{1}", dFile, new string(' ', consoleWidth - (dFile.Length)));
                    if (!unitTest) Console.WriteLine(fileInfo);
                    if (downloadedFiles[i].GetDecompressedFileName() != null) decompressedFiles.Add(downloadedFiles[i].GetDecompressedFileName());
                }
                if (decompressedFiles.Count > 0) WriteHeader(HeaderType.decompressedHeader);
                for (int i = 0; i < decompressedFiles.Count; i++)
                {
                    string fileInfo = String.Format(@"Unzipped file: {0}", decompressedFiles[i]);
                    if (!unitTest) Console.WriteLine(fileInfo);
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region ENTRIES
        static public void ReportCondensedEntries()
        {
            try
            {
                if (!unitTest) Console.Clear();
                WriteHeader(HeaderType.mainHeader);
                CONSOLE_MESSAGE = "Condensing results";
                for (int i = 0; i < 5; i++)
                {
                    if (!unitTest) Console.Write(CONSOLE_MESSAGE);
                    Thread.Sleep(250);
                    CONSOLE_MESSAGE = ".";
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        static public void PrintEntries(List<ViewCountEntry> viewCountEntries, int maxDomainLength, int maxPageTitleLength, int maxViewCountLength, int currentTablePage = 0, int maxTablePage = 0)
        {
            try
            {
                if (!unitTest) Console.Clear();
                WriteHeader(HeaderType.mainHeader);

                string domain = String.Format(@"{0}{1}", "DOMAIN", new string(' ', Math.Max(maxDomainLength - "DOMAIN".Length, 0)));
                string pageTitle = String.Format(@"{0}{1}", "PAGE TITLE", new string(' ', Math.Max(maxPageTitleLength - "PAGE TITLE".Length, 0)));
                string viewCount = String.Format(@"{0}{1}", "VIEW COUNT", new string(' ', Math.Max(maxViewCountLength - "VIEW COUNT".Length, 0)));

                string tableTitle = String.Format(@"{0}|{1}|{2}", domain, pageTitle, viewCount);
                string breakerLine = new string('=', tableTitle.Length);
                if (!unitTest)
                {
                    Console.WriteLine(tableTitle);
                    Console.WriteLine(breakerLine);
                }

                for (int i = 0; i < Constants.MAX_ENTRIES_PER_PAGE; i++)
                {
                    int index = currentTablePage * Constants.MAX_ENTRIES_PER_PAGE + i;
                    if (index >= viewCountEntries.Count) return;
                    var entry = viewCountEntries[index];
                    domain = String.Format(@"{0}{1}", entry.GetDomain(), new string(' ', Math.Max(maxDomainLength - entry.GetDomain().Length, 0)));
                    pageTitle = String.Format(@"{0}{1}", entry.GetPageTitle(), new string(' ', Math.Max(maxPageTitleLength - entry.GetPageTitle().Length, 0)));
                    viewCount = String.Format(@"{0}{1}", entry.GetViewCount(), new string(' ', Math.Max(maxViewCountLength - entry.GetViewCount().ToString().Length, 0)));

                    string entryInfo = String.Format(@"{0}|{1}|{2}", domain, pageTitle, viewCount);
                    if (!unitTest) Console.WriteLine(entryInfo);
                }

                string exitInfo = "ESC to exit";
                string pageInfo = ((currentTablePage + 1).ToString() + " | " + maxTablePage.ToString());
                string pageOffset = new string(' ', tableTitle.Length - (exitInfo.Length + pageInfo.Length));
                string pageLine = String.Format(@"{0}{1}{2}", exitInfo, pageOffset, pageInfo);
                if (!unitTest)
                {
                    Console.WriteLine(breakerLine);
                    Console.WriteLine(pageLine);
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion
    }
}
