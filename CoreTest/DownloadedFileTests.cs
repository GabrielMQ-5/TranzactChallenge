using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Entities;

namespace CoreTest
{
    [TestClass]
    public class DownloadedFileTests
    {
        [TestMethod]
        public void DownloadedFile_Construction_Test()
        {
            string fileName = "TestName";
            
            DownloadedFile downloadedFile = new DownloadedFile(fileName);
            
            Assert.AreEqual(downloadedFile.GetFileName(), fileName, "File name not properly set");            
        } 
        
        [TestMethod]
        public void DownloadedFile_SetValues_Test()
        {
            int totalEntries = 10;
            string fileName = "TestName";
            string decompressedName = "TestName";

            DownloadedFile downloadedFile = new DownloadedFile(fileName);
            downloadedFile.SetDecompressedFileName(decompressedName);
            downloadedFile.SetTotalEntries(totalEntries);

            Assert.IsNotNull(downloadedFile.GetDecompressedFileName(), "Decompressed file name not properly set");
            Assert.AreEqual(downloadedFile.GetDecompressedFileName(), decompressedName, "Decompressed file name not properly set");
            Assert.AreEqual(downloadedFile.GetTotalEntries(), totalEntries, 0.001, "Entries not properly set");
        }
        [TestMethod]
        public void DownloadedFile_UpdatePercentage_Test()
        {
            int totalEntries = 10;
            int startingEntries = 5;
            double expectedPercentage = 100;
            string fileName = "TestName";
            
            DownloadedFile downloadedFile = new DownloadedFile(fileName, startingEntries);
            downloadedFile.SetTotalEntries(totalEntries);

            for (int i = 0; i <= totalEntries - startingEntries; i++)
            {
                downloadedFile.AddPercentage();
            }

            Assert.AreEqual(downloadedFile.GetPercentage(), expectedPercentage, 0.001, "Percentage not properly updated");
        }
    }
}
