using Core.Constants;
using Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServiceTest
{
    [TestClass]
    public class WikimediaServiceTests
    {
        [TestMethod]
        public void WikimediaService_DownloadFile_Test()
        {
            WikimediaService service = new WikimediaService();
            string fileName = "testBatch.gz";
            string fileUrl = Path.Combine(Environment.CurrentDirectory, fileName);

            service.Download(fileUrl, fileName, false);
            service.WaitDownloads();
        }

        [TestMethod]
        public void WikimediaService_ProcessFile_Test()
        {
            WikimediaService service = new WikimediaService();
            string fileName = "testBatch.gz";
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            string dumpsPath = Path.Combine(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Constants.APPLICATION_FOLDER),
                Constants.DUMPS_FOLDER);
            string copyPath = Path.Combine(dumpsPath, fileName);

            DownloadedFile dFile = new DownloadedFile(fileName);
            File.Copy(filePath, copyPath, true);

            service.Decompress(dFile);
            service.ExtractData(dFile);
            service.WaitReads();
        }

        [TestMethod]
        public void WikimediaService_ObtainResults_Test()
        {
            ServiceHelper.unitTest = true;
            List<ViewCountEntry> entries = new();
            string domain = "test";
            string pageTitle = "TEST-";

            for (int i = 0; i < 5; i++)
            {
                entries.Add(new ViewCountEntry(domain, (pageTitle + i), i * 5));
            }

            WikimediaService service = new WikimediaService(entries);

            service.ObtainTopResults();
            service.DisplayTopResults();
        }
    }
}
