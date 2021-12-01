using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Entities;

namespace CoreTest
{
    [TestClass]
    public class FileDownloadResultTests
    {
        [TestMethod]
        public void FileDownloadResult_Construction_Test()
        {
            string fileName = "TestName";
            string testResult = "RESULT";
            string testError = "ERROR";

            FileDownloadResult fileDownloadResultA = new FileDownloadResult(fileName, testResult);
            FileDownloadResult fileDownloadResultB = new FileDownloadResult(fileName, testResult, testError);

            Assert.AreEqual(fileDownloadResultA.GetFileName(), fileName, "File name not properly set");
            Assert.AreEqual(fileDownloadResultA.GetResult(), testResult, "Result not properly set");
            
            Assert.IsNull(fileDownloadResultA.GetError(), "Error not properly set");
            Assert.IsNotNull(fileDownloadResultB.GetError(), "Error not properly set");
            Assert.AreEqual(fileDownloadResultB.GetError(), testError, "Error not properly set");
        }
        [TestMethod]
        public void FileDownloadResult_UpdateRetries_Test()
        {
            int totalRetries = 10;
            string fileName = "TestName";
            string testResult = "RESULT";
            
            FileDownloadResult fileDownloadResultA = new FileDownloadResult(fileName, testResult);
            
            for (int i = 0; i < totalRetries; i++)
            {
                fileDownloadResultA.AddRetries();
            }

            Assert.AreEqual(fileDownloadResultA.GetRetries(), totalRetries, 0.001, "Retries not properly updated");
        }
    }
}
