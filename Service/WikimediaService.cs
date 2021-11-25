using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Core.Constants;

namespace Service
{
    public class WikimediaService
    {
        private bool _result = false;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly string documentsPath;
        private readonly string applicationPath;
        private readonly string dumpsPath;
        private readonly string unzippedPath;

        public WikimediaService()
        {
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            applicationPath = Path.Combine(documentsPath, Constants.APPLICATION_FOLDER);
            dumpsPath = Path.Combine(applicationPath, Constants.DUMPS_FOLDER);
            unzippedPath = Path.Combine(applicationPath, Constants.UNZIPPED_FOLDER);
        }

        public bool StartDownload(DownloadProgressChangedEventHandler updateDelegate, string fileUrl, string fileName, int timeout = 1 * 60 * 1000)
        {
            try
            {
                Container container = new();
                string filePath = Path.Combine(dumpsPath, fileName);
                if (!File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (WebClient client = new WebClient())
                {
                    var url = new Uri(String.Concat(ApiUrl.BaseUrl, fileUrl, fileName));
                    client.Headers["filename"] = fileName;
                    client.DownloadProgressChanged += updateDelegate;
                    client.DownloadFileCompleted += WebClientDownloadCompleted;
                    Console.WriteLine(@"Downloading file: {0}", fileName);
                    client.DownloadFileAsync(url, dumpsPath + '\\' + fileName);
                    _semaphore.Wait(timeout);
                    return _result && File.Exists(dumpsPath + '\\' + fileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Was not able to download file!");
                Console.Write(e.Message);
                return false;
            }
            finally
            {
                this._semaphore.Dispose();
            }
        }

        private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            _result = !args.Cancelled || args.Error != null;
            if (!_result)
            {
                Console.Write(args.Error.ToString());
            }
            else
            {
                Console.WriteLine(Environment.NewLine + "Download finished!");
            }
            try
            {
                _semaphore.Release();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}
