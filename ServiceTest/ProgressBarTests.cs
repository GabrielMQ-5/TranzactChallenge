using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Helper;
using System.Threading;

namespace ServiceTest
{
    [TestClass]
    public class ProgressBarTests
    {
        [TestMethod]
        public void ProgressBar_Construction_Test()
        {
            string testFileName = "FileName";

            ProgressBar progressBar = new ProgressBar(testFileName);
            Thread.Sleep(150);
            progressBar.Dispose();

            Assert.AreEqual(testFileName, progressBar.GetRelatedFile(), "File name not properly set");
            Assert.AreNotEqual(string.Empty, progressBar.GetCurrentText(), "Current text not properly updated");
        }

        [TestMethod]
        public void ProgressBar_Report_Test()
        {
            string testFileName = "FileName";

            ProgressBar progressBar = new ProgressBar(testFileName);
            progressBar.Report(1);
            Thread.Sleep(150);
            progressBar.Dispose();

            Assert.AreNotEqual(string.Empty, progressBar.GetCurrentText(), "Current text not properly updated");
            Assert.IsTrue(progressBar.GetCurrentText().Contains("100"), "Current text not properly updated");
        }
    }
}
