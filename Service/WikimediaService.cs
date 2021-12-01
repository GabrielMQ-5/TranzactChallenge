using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Constants;
using Core.Entities;
using Service.Helper;

namespace Service
{
    public class WikimediaService
    {
        private readonly string documentsPath;
        private readonly string applicationPath;
        private readonly string dumpsPath;
        private readonly string unzippedPath;
        private readonly List<Task> fileDownloadTasks;
        private readonly List<Task> fileReadTasks;
        private readonly List<DateTime> lastFiles;
        private readonly List<ProgressBar> progressBars;
        private readonly List<FileDownloadResult> fileResults;
        private readonly List<DownloadedFile> downloadedFiles;
        private List<ViewCountEntry> viewCountEntries;
        private DateTime timestamp;
        private int totalRetries = 0;
        private int fileRetries = 0;
        private int fileResultIndex = 0;
        private enum ProcessStage { Download = 0, Decompress = 1, Extract = 2, Condense = 3, Display = 4, Disposed = 5 }
        private ProcessStage currentStage;

        private int maxDomainLength = 0;
        private int maxPageTitleLength = 0;
        private int maxViewCountLength = 0;
        private int maxTablePage = 0;
        private int currentTablePage = 0;
        private ConsoleKey nextPageKey = ConsoleKey.RightArrow;
        private ConsoleKey prevPageKey = ConsoleKey.LeftArrow;
        private ConsoleKey exitKey = ConsoleKey.Escape;

        public static List<string> exceptionsEncountered = new();

        public WikimediaService(List<ViewCountEntry> viewCountEntries = null)
        {
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            applicationPath = Path.Combine(documentsPath, Constants.APPLICATION_FOLDER);
            dumpsPath = Path.Combine(applicationPath, Constants.DUMPS_FOLDER);
            unzippedPath = Path.Combine(applicationPath, Constants.UNZIPPED_FOLDER);
            lastFiles = new List<DateTime>();
            fileDownloadTasks = new List<Task>();
            fileReadTasks = new List<Task>();
            progressBars = new List<ProgressBar>();
            fileResults = new List<FileDownloadResult>();
            this.viewCountEntries = (viewCountEntries != null) ? viewCountEntries : new List<ViewCountEntry>();
            downloadedFiles = new List<DownloadedFile>();
            timestamp = ServiceHelper.GetCurrentTime();
            currentStage = ProcessStage.Download;
        }

        #region UTIL
        public void PrintConsole()
        {
            Task.Run(() =>
            {
                do
                {
                    ServiceHelper.ReportFoundFiles(lastFiles, progressBars, fileResults);
                    Thread.Sleep(125);
                } while (currentStage == ProcessStage.Download);
                ServiceHelper.ResetScreen();
                do
                {
                    ServiceHelper.ReportDecompressedFiles(downloadedFiles);
                    Thread.Sleep(250);
                } while (currentStage == ProcessStage.Decompress || currentStage == ProcessStage.Extract);
                do
                {
                    ServiceHelper.ReportCondensedEntries();
                }
                while (currentStage == ProcessStage.Condense);
            });
        }

        public void WaitDownloads()
        {
            try
            {
                Task.WaitAll(fileDownloadTasks.ToArray());
                fileDownloadTasks.Clear();
                currentStage = ProcessStage.Decompress;
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        public void WaitReads()
        {
            try
            {
                while (fileReadTasks.Find(x => x.IsCompleted == false) != null)
                {
                    Thread.Sleep(500);
                }
                fileReadTasks.Clear();
                currentStage = ProcessStage.Condense;
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region DOWNLOAD
        public void FindLastFiles(int maxFiles = Constants.MAX_FILES)
        {
            maxFiles = Math.Max(maxFiles, 1);
            while (lastFiles.Count < maxFiles && totalRetries < Constants.MAX_RETRIES)
            {
                string fileUrl = ServiceHelper.FormatFileUrl(timestamp);
                string fileName = ServiceHelper.FormatFileName(timestamp);
                string fullPath = String.Concat(ApiUrl.BaseUrl, fileUrl, fileName);
                Download(fullPath, fileName);
            }
        }

        public void Download(string fileUrl, string fileName, bool delayRequest = true)
        {
            try
            {
                WebClient client = new WebClient();
                var url = new Uri(fileUrl);
                client.DownloadProgressChanged += WebClientDownloadProgress;
                client.DownloadFileCompleted += WebClientDownloadCompleted;
                client.Headers["fileName"] = fileName;
                fileDownloadTasks.Add(client.DownloadFileTaskAsync(url, dumpsPath + '\\' + fileName));
                if (delayRequest) ServiceHelper.DelayRequest();

                if (client.IsBusy)
                {
                    lastFiles.Add(timestamp);
                    progressBars.Add(new ProgressBar(fileName));
                    timestamp = timestamp.AddHours(-1);
                    fileRetries = 0;
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }
        #endregion

        #region PROCESS
        public void ProcessFiles()
        {
            foreach (var fileToDecompress in downloadedFiles)
            {
                Decompress(fileToDecompress);
            }
            currentStage = ProcessStage.Extract;
            foreach (var fileToProcess in downloadedFiles)
            {
                ExtractData(fileToProcess);
            }
        }

        public void Decompress(DownloadedFile currentFile)
        {
            FileInfo fileToDecompress = new FileInfo(dumpsPath + '\\' + currentFile.GetFileName());
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.Name;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length) + ".txt";
                string newFilePath = unzippedPath + '\\' + newFileName;

                using (FileStream decompressedFileStream = File.Create(newFilePath))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }

                currentFile.SetDecompressedFileName(newFileName);
                currentFile.SetTotalEntries(File.ReadLines(newFilePath).Count());
                File.Delete(dumpsPath + '\\' + currentFile);
            }
        }

        public void ExtractData(DownloadedFile downloadedFile)
        {
            string fileToProcess = unzippedPath + '\\' + downloadedFile.GetDecompressedFileName();
            Task readTask = Task.Run(() =>
            {
                foreach (string entry in File.ReadLines(fileToProcess))
                {
                    string[] values = entry.Split(" ");
                    if (values.Length < 3) continue;
                    if (!Int32.TryParse(values[2], out int viewCount)) continue;
                    if (viewCount == 0) continue;
                    try
                    {
                        (string domain, string pageTitle) = (values[0], values[1]);
                        viewCountEntries.Add(new ViewCountEntry(domain, pageTitle, viewCount));
                    }
                    catch (Exception ex)
                    {
                        exceptionsEncountered.Add(ex.Message);
                    }
                    downloadedFile.AddPercentage();
                }
                File.Delete(fileToProcess);
            });
            fileReadTasks.Add(readTask);
        }
        #endregion

        #region DISPLAY
        public void ObtainTopResults()
        {
            viewCountEntries = viewCountEntries.GroupBy(x => new { domain = x?.GetDomain(), pageTitle = x?.GetPageTitle() })
                .Select(sum => new ViewCountEntry(sum.Key.domain, sum.Key.pageTitle, sum.Sum(s => s != null ? s.GetViewCount() : 0)))
                .OrderByDescending(x => x.GetViewCount())
                .Take(100).ToList();
            currentStage = ProcessStage.Display;
        }

        public void DisplayTopResults()
        {
            maxDomainLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.GetDomain().Length : 0) ? max : cur.GetDomain()).Length;
            maxPageTitleLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.GetPageTitle().Length : 0) ? max : cur.GetPageTitle()).Length;
            maxViewCountLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.GetViewCount().ToString().Length : 0) ? max : cur.GetViewCount().ToString()).Length;
            maxTablePage = viewCountEntries.Count / Constants.MAX_ENTRIES_PER_PAGE + (viewCountEntries.Count % Constants.MAX_ENTRIES_PER_PAGE > 0 ? 1 : 0);
            ServiceHelper.PrintEntries(viewCountEntries, maxDomainLength, maxPageTitleLength, maxViewCountLength, currentTablePage, maxTablePage);
        }

        public void NavigateResults()
        {
            while (currentStage == ProcessStage.Display)
            {
                var input = Console.ReadKey();
                if (input.Key == exitKey) break;
                if (input.Key == nextPageKey && currentTablePage < maxTablePage - 1)
                {
                    ServiceHelper.PrintEntries(viewCountEntries, maxDomainLength, maxPageTitleLength, maxViewCountLength, ++currentTablePage, maxTablePage);
                }
                else if (input.Key == prevPageKey && currentTablePage > 0)
                {
                    ServiceHelper.PrintEntries(viewCountEntries, maxDomainLength, maxPageTitleLength, maxViewCountLength, --currentTablePage, maxTablePage);
                }
            }
            currentStage = ProcessStage.Disposed;
        }
        #endregion

        #region DOWNLOAD HANDLERS
        private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            try
            {
                WebClient client = (WebClient)sender;
                string fileName = client.Headers["fileName"];
                if (args.Error != null)
                {
                    WebException exception = (WebException)args.Error;
                    if (exception.Response == null)
                    {
                        timestamp = timestamp.AddHours(-1);
                        totalRetries++;
                        fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_ERRORED, exception.Message));
                        return;
                    }
                    HttpWebResponse response = (HttpWebResponse)exception.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        timestamp = timestamp.AddHours(-1);
                        fileRetries = 0;
                        totalRetries++;
                        fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_NOT_FOUND, "404"));
                        ServiceHelper.DelayRequest();
                    }
                    else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        if (fileRetries == 0)
                        {
                            fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_SERVICE_UNAVAILABLE, "503"));
                            fileResultIndex = fileResults.FindLastIndex(x => x.GetFileName() == fileName);
                        }
                        if (fileRetries < Constants.MAX_FILE_RETRIES)
                        {
                            fileRetries++;
                            fileResults[fileResultIndex].AddRetries();
                            ServiceHelper.DelayRequest(fileRetries);
                        }
                        else
                        {
                            timestamp = timestamp.AddHours(-1);
                            fileRetries = 0;
                            totalRetries++;
                            ServiceHelper.DelayRequest();
                        }
                    }
                }
                else if (args.Cancelled)
                {
                    fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_CANCELLED));
                }
                else
                {
                    fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_SUCCESS));
                    downloadedFiles.Add(new DownloadedFile(fileName));
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void WebClientDownloadProgress(object sender, DownloadProgressChangedEventArgs args)
        {
            var (bytesReceived, totalBytesToReceive) = (Convert.ToDouble(args.BytesReceived), Convert.ToDouble(args.TotalBytesToReceive));
            WebClient client = (WebClient)sender;
            ProgressBar prBar = progressBars.Find(x => x.GetRelatedFile() == client.Headers["fileName"]);
            if (prBar == null) return;
            prBar.Report(Math.Round(bytesReceived / totalBytesToReceive, 2));
        }
        #endregion
    }
}
