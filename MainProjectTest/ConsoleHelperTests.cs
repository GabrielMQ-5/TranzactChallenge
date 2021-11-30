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
            ConsoleHelper.SetCurrentFont("Verdana");

            Assert.IsNotNull(ConsoleHelper.exceptionsEncountered, "Exception collector not properly set");
            Assert.IsTrue(ConsoleHelper.exceptionsEncountered.Count == 0, "Encountered exceptions during testing");
            ConsoleHelper.exceptionsEncountered.Clear();
        }
    }
}
