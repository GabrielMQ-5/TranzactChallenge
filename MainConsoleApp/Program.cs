using System;
using Service;
using MainConsoleApp.Helper;
using System.Text;

namespace MainConsoleApp
{
    class Program
    {
        static void Main()
        {
            DateTime start = DateTime.Now;
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.Unicode;
            ConsoleHelper.SetCurrentFont("Consolas", 10);
            MainHelper.SetupFolderStructure();
            MainHelper.CleanUpFolders();
            WikimediaService wmService = new();
            wmService.PrintConsole();
            wmService.FindLastFiles();
            wmService.WaitDownloads();
            wmService.ProcessFiles();
            wmService.WaitReads();
            wmService.ObtainTopResults();
            wmService.DisplayTopResults();

            DateTime finish = DateTime.Now;
            Console.WriteLine(@"{0} | {1}", start.ToShortTimeString(), finish.ToShortTimeString());
        }
    }
}
