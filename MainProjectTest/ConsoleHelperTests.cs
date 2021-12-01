using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainConsoleApp.Helper;

namespace MainProjectTest
{
    [TestClass]
    public class ConsoleHelperTests
    {
        [TestMethod]
        public void UpdateFont_Test()
        {
            //Since the function executes on console runtime it seemingly cannot be properly tested using conventional unit test
            //ConsoleHelper.SetCurrentFont("Consolas");

            Assert.IsNotNull(ConsoleHelper.exceptionsEncountered, "Exception collector not properly set");
            Assert.IsTrue(ConsoleHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            ConsoleHelper.exceptionsEncountered.Clear();
        }
    }
}
