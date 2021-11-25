using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Service;
using Service.Helper;
using MainConsoleApp.Helper;

namespace MainConsoleApp
{
    class Program
    {
        static List<string> fileNameList;
        static List<string> fileUrlList;

        static void Main()
        {
            Console.CursorVisible = false;
            MainHelper.SetupFolderStructure();
            ServiceHelper.FindLastFiles();
            fileNameList = ServiceHelper.GetFilenames();
            fileUrlList = ServiceHelper.GetUrls();

            for (int i = 0; i < ServiceHelper.MAX_FILES; i++)
            {
                WikimediaService wmService = new();
                wmService.StartDownload(fileUrlList[i], fileNameList[i]);
            }
        }
    }
}
