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
        private enum ProcessStage { Download = 0, Decompress = 1, Extract = 2, Condense = 3, Display = 4 }
        private ProcessStage currentStage;

        public WikimediaService()
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
            viewCountEntries = new List<ViewCountEntry>();
            downloadedFiles = new List<DownloadedFile>();
            timestamp = ServiceHelper.GetCurrentTime();
            currentStage = ProcessStage.Download;
        }

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

        public void FindLastFiles()
        {
            while (lastFiles.Count < Constants.MAX_FILES && totalRetries < Constants.MAX_RETRIES)
            {
                string fileUrl = ServiceHelper.FormatFileUrl(timestamp);
                string fileName = ServiceHelper.FormatFileName(timestamp);
                try
                {
                    WebClient client = new WebClient();
                    var url = new Uri(String.Concat(ApiUrl.BaseUrl, fileUrl, fileName));
                    client.DownloadProgressChanged += WebClientDownloadProgress;
                    client.DownloadFileCompleted += WebClientDownloadCompleted;
                    client.Headers["fileName"] = fileName;
                    fileDownloadTasks.Add(client.DownloadFileTaskAsync(url, dumpsPath + '\\' + fileName));
                    ServiceHelper.DelayRequest();

                    if (client.IsBusy)
                    {
                        lastFiles.Add(timestamp);
                        progressBars.Add(new ProgressBar(fileName));
                        timestamp = timestamp.AddHours(-1);
                        fileRetries = 0;
                    }
                }
                catch (Exception ex) { }
            }
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

            }
        }

        public void ObtainTopResults()
        {
            viewCountEntries = viewCountEntries.GroupBy(x => new { domain = x != null ? x.domain : null, pageTitle = x != null ? x.pageTitle : null })
                .Select(sum => new ViewCountEntry(sum.Key.domain, sum.Key.pageTitle, sum.Sum(s => s != null ? s.viewCount : 0)))
                .OrderByDescending(x => x.viewCount)
                .Take(100).ToList();
            currentStage = ProcessStage.Display;
        }

        public void DisplayTopResults()
        {
            int maxDomainLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.domain.Length : 0) ? max : cur.domain).Length;
            int maxPageTitleLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.pageTitle.Length : 0) ? max : cur.pageTitle).Length;
            int maxViewCountLength = viewCountEntries.Aggregate("", (max, cur) => max.Length > (cur != null ? cur.viewCount.ToString().Length : 0) ? max : cur.viewCount.ToString()).Length;
            ServiceHelper.PrintEntries(viewCountEntries, maxDomainLength, maxPageTitleLength, maxViewCountLength);
        }


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
                            fileResultIndex = fileResults.FindLastIndex(x => x.fileName == fileName);
                        }
                        if (fileRetries < Constants.MAX_FILE_RETRIES)
                        {
                            fileRetries++;
                            fileResults[fileResultIndex].retries = fileRetries;
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
                    fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_CANCELLED, null));
                }
                else
                {
                    fileResults.Add(new FileDownloadResult(fileName, Constants.FILE_RESULT_SUCCESS, null));
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
            ProgressBar prBar = progressBars.Find(x => x.relatedFile == client.Headers["fileName"]);
            if (prBar == null) return;
            prBar.Report(Math.Round(bytesReceived / totalBytesToReceive, 2));
        }

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
            FileInfo fileToDecompress = new FileInfo(dumpsPath + '\\' + currentFile.fileName);
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

                currentFile.decompressedFileName = newFileName;
                currentFile.totalEntries = File.ReadLines(newFilePath).Count();
                File.Delete(dumpsPath + '\\' + currentFile);
            }
        }

        public void ExtractData(DownloadedFile downloadedFile)
        {
            string fileToProcess = unzippedPath + '\\' + downloadedFile.decompressedFileName;
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
                    catch (Exception ex) { }
                    downloadedFile.AddPercentage();
                }
                File.Delete(fileToProcess);
            });
            fileReadTasks.Add(readTask);
        }
    }
}
