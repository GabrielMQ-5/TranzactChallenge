using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Service;
using Service.Helper;
using MainConsoleApp.DisplayElements;
using MainConsoleApp.Helper;

namespace MainConsoleApp
{
    class Program
    {
        static List<string> fileNameList;
        static List<string> fileUrlList;
        static List<ProgressBar> progressBarList;

        static void Main()
        {
            MainHelper.SetupFolderStructure();
            ServiceHelper.FindLastFiles();
            fileNameList = ServiceHelper.GetFilenames();
            fileUrlList = ServiceHelper.GetUrls();
            progressBarList = new List<ProgressBar>();

            for (int i = 0; i < 3; i++)
            {
                progressBarList.Add(new ProgressBar(fileNameList[i], i));
                WikimediaService wmService = new();
                wmService.StartDownload(WebClientDownloadProgressChanged, fileUrlList[i], fileNameList[i]);
            }
        }

        private static void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            WebClient client = sender as WebClient;
            string filename = client.Headers["filename"];
            var prBar = progressBarList.Find(x => x.relatedFile == filename);
            using (prBar)
            {
                for (int i = 0; i <= 100; i++)
                {
                    prBar.Report(Math.Round(Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive), 2));
                    Thread.Sleep(20);
                }
            }
        }
    }
}
