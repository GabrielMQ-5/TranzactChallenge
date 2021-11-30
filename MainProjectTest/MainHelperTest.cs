using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainConsoleApp.Helper;

namespace MainProjectTest
{
    [TestClass]
    public class MainHelperTests
    {
        [TestMethod]
        public void SetupFolders_Test()
        {
            MainHelper.SetupFolderStructure();
            
            Assert.IsNotNull(MainHelper.exceptionsEncountered, "Exception collector not properly set");
            Assert.IsTrue(MainHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            MainHelper.exceptionsEncountered.Clear();
        }
        
        [TestMethod]
        public void CealUpFolders_Test()
        {
            MainHelper.CleanUpFolders();

            Assert.IsNotNull(MainHelper.exceptionsEncountered, "Exception collector not properly set");
            Assert.IsTrue(MainHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            MainHelper.exceptionsEncountered.Clear();
        }
    }
}
