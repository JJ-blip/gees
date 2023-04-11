using LsideWPF.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LsideWPFTest.Models
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class StateUtilTest
    {
        public StateUtilTest()
        {
        }

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestGetDistance()
        {
            double longitude = 51.1481753198707;
            double latitude = -0.190913448221807;
            double otherLongitude = 51.1470638495108;
            double otherLatitude = -0.197356281433674;
            double result = StateUtil.GetDistance(longitude, latitude, otherLongitude, otherLatitude);
            Assert.IsTrue(result.InRange(720, 730));
        }
    }
}
