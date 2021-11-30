using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Entities;

namespace CoreTest
{
    [TestClass]
    public class ViewCountEntryTests
    {
        [TestMethod]
        public void ViewCountEntry_Construction_Test()
        {
            string domain = "Domain";
            string pageTitle = "PageTitle";
            int viewCount = 10;
            
            ViewCountEntry viewCountEntry = new ViewCountEntry(domain, pageTitle, viewCount);
            
            Assert.AreEqual(viewCountEntry.GetDomain(), domain, "Domain not properly set");
            Assert.AreEqual(viewCountEntry.GetPageTitle(), pageTitle, "Page title not properly set");
            Assert.AreEqual(viewCountEntry.GetViewCount(), viewCount, "View count not properly set");
        }
    }
}
