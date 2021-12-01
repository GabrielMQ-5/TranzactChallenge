using System;
using System.Collections.Generic;
using System.IO;
using Core.Constants;

namespace MainConsoleApp.Helper
{
    public static class MainHelper
    {
        private static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string applicationPath = Path.Combine(documentsPath, Constants.APPLICATION_FOLDER);
        private static string dumpsPath = Path.Combine(applicationPath, Constants.DUMPS_FOLDER);
        private static string unzippedPath = Path.Combine(applicationPath, Constants.UNZIPPED_FOLDER);

        public static List<string> exceptionsEncountered = new();

        public static void SetupFolderStructure()
        {
            try
            {
                Directory.CreateDirectory(applicationPath);
                Directory.CreateDirectory(dumpsPath);
                Directory.CreateDirectory(unzippedPath);
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        public static void CleanUpFolders()
        {
            try
            {
                if (Directory.Exists(applicationPath))
                {
                    if (Directory.Exists(dumpsPath))
                    {
                        foreach (var file in Directory.GetFiles(dumpsPath))
                        {
                            File.Delete(file);
                        }
                    }
                    if (Directory.Exists(unzippedPath))
                    {
                        foreach (var file in Directory.GetFiles(unzippedPath))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }

        }
    }
}
