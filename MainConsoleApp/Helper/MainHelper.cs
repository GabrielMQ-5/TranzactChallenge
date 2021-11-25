using System;
using System.IO;
using Core.Constants;

namespace MainConsoleApp.Helper
{
    public static class MainHelper
    {
        public static void SetupFolderStructure()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationPath = Path.Combine(documentsPath, Constants.APPLICATION_FOLDER);
            string dumpsPath = Path.Combine(applicationPath, Constants.DUMPS_FOLDER);
            string unzippedPath = Path.Combine(applicationPath, Constants.UNZIPPED_FOLDER);

            Directory.CreateDirectory(applicationPath);
            Directory.CreateDirectory(dumpsPath);
            Directory.CreateDirectory(unzippedPath);
        }
    }
}
