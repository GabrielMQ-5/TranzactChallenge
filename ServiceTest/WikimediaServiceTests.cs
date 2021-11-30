using Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service;

namespace ServiceTest
{
    [TestClass]
    public class WikimediaServiceTests
    {
        WikimediaService service = new WikimediaService();
        [TestMethod]
        public void WikimediaService_DownloadFile_Test()
        {
            service.FindLastFiles(1);
            service.WaitDownloads();
        }

        [TestMethod]
        public void WikimediaService_ProcessFile_Test()
        {
            service.ProcessFiles();
            service.WaitReads();
        }

        [TestMethod]
        public void WikimediaService_ObtainResults_Test()
        {
            service.ObtainTopResults();
            service.DisplayTopResults();
        }
    }
}
