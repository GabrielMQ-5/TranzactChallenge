using Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Helper;
using System;
using System.Collections.Generic;

namespace ServiceTest
{
    [TestClass]
    public class ServiceHelperTests
    {
        [TestMethod]
        public void ServiceHelper_DirectConsolePrintFunctions_Test()
        {
            // Console calls raise exception whenever they are called within a unit test.
            // It is an issue with certain Visual Studio versions.
            ServiceHelper.unitTest = true;

            ServiceHelper.DelayRequest(0, 0);
            ServiceHelper.ResetScreen();
            ServiceHelper.ReportCondensedEntries();

            foreach (ServiceHelper.HeaderType headerType in (ServiceHelper.HeaderType[])Enum.GetValues(typeof(ServiceHelper.HeaderType)))
            {
                ServiceHelper.WriteHeader(headerType);
            }

            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_GetCurrentTime_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime _tStamp;
            DateTime tStamp;

            _tStamp = DateTime.UtcNow.AddHours(-1);
            tStamp = new DateTime(_tStamp.Year, _tStamp.Month, _tStamp.Day, _tStamp.Hour, 0, 0);
            DateTime timestamp = ServiceHelper.GetCurrentTime();

            Assert.AreEqual(tStamp, timestamp, "Timestamps are not equal");
            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_GetFileName_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime testTime = new DateTime(2021, 1, 1, 0, 0, 0);
            string testFileName = "pageviews-20210101-000000.gz";
            List<DateTime> tTimes = new();

            tTimes.Add(testTime);
            string fileNameA = ServiceHelper.GetFilenames(tTimes)[0];
            string fileNameB = ServiceHelper.FormatFileName(testTime);

            Assert.AreEqual(testFileName, fileNameA, "File names are not equal");
            Assert.AreEqual(testFileName, fileNameB, "File names are not equal");
            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_GetFileUrl_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime testTime = new DateTime(2021, 1, 1, 0, 0, 0);
            string testFileUrl = "2021/2021-01/";
            List<DateTime> tTimes = new();

            tTimes.Add(testTime);
            string urlA = ServiceHelper.GetUrls(tTimes)[0];
            string urlB = ServiceHelper.FormatFileUrl(testTime);

            Assert.AreEqual(testFileUrl, urlA, "File urls are not equal");
            Assert.AreEqual(testFileUrl, urlB, "File urls are not equal");
            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_PrintViewCountEntries_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime testTime = new DateTime(2021, 1, 1, 0, 0, 0);
            List<DateTime> tTimes = new();
            string tDomain = "Domain";
            string tPageTitle = "PageTitle";
            int tViewCount = 10;
            List<ViewCountEntry> tEntries = new();

            tTimes.Add(testTime);
            tEntries.Add(new ViewCountEntry(tDomain, tPageTitle, tViewCount));
            ServiceHelper.PrintEntries(tEntries, tDomain.Length, tPageTitle.Length, tViewCount.ToString().Length);

            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_ReportDownloadedFiles_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime testTime = new DateTime(2021, 1, 1, 0, 0, 0);
            List<DateTime> tTimes = new();
            string tFileName = "FileName";
            List<DownloadedFile> tFiles = new();

            tTimes.Add(testTime);
            tFiles.Add(new DownloadedFile(tFileName));
            ServiceHelper.ReportDecompressedFiles(tFiles);

            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ServiceHelper_ReportFileDownloadResults_Test()
        {
            ServiceHelper.unitTest = true;

            DateTime testTime = new DateTime(2021, 1, 1, 0, 0, 0);
            List<DateTime> tTimes = new();
            string tFileName = "FileName";
            string tResult = "RESULT";
            List<FileDownloadResult> tResults = new();
            List<ProgressBar> tProgressBars = new();

            tTimes.Add(testTime);
            tResults.Add(new FileDownloadResult(tFileName, tResult));
            tProgressBars.Add(new ProgressBar(tFileName));
            ServiceHelper.ReportFoundFiles(tTimes, tProgressBars, tResults);

            try
            {
                Assert.IsTrue(ServiceHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            }
            catch (Exception ex)
            {
                ServiceHelper.exceptionsEncountered.Clear();
                Assert.Fail(ex.Message);
            }
        }
    }
}
