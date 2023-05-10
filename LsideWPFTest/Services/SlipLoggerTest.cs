using LsideWPF.Common;
using LsideWPF.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LsideWPFTest.Services
{
    [TestClass]
    public class SlipLoggerTest
    {
        private readonly SlipLogger slipLogger = new SlipLogger();

        [TestMethod]
        public void TestGetSlipAngle()
        {
            var response = new PlaneInfoResponse()
            {
                RelativeWindX = 3,
                RelativeWindZ = 4,
            };

            var uut = new PrivateObject(slipLogger);
            var output = (double)uut.Invoke("GetSlipAngle", response);

            // 36
            Assert.IsTrue(Between(output, 36.85, 36.87));
        }

        [TestMethod]
        public void TestGetCenterLineOffsetDegrees()
        {
            var response = new PlaneInfoResponse()
            {
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 257.5948486328125,
                GpsGroundTrueHeading = 266.99102914628781,
            };

            var uut = new PrivateObject(slipLogger);

            // +9.39
            var clo = (double)uut.Invoke("GetCenterLineOffsetDegrees", response);

            Assert.IsTrue(Between(clo, 9.38, 9.40));
        }

        [TestMethod]
        public void TestGetCenterLineOffsetDegrees2()
        {
            var response = new PlaneInfoResponse()
            {
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 360 - 20,
            };

            var uut = new PrivateObject(slipLogger);

            // -20
            var clo = (double)uut.Invoke("GetCenterLineOffsetDegrees", response);

            Assert.IsTrue(Between(clo, -21, -19));
        }

     
        [TestMethod]
        public void TestGetSlipComponents3()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on right, + slip to left
                RelativeWindX = -3,
                RelativeWindZ = 4,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(Between(slip, -36.87, -36.85));
        }

        [TestMethod]
        public void TestGetSlipComponents5()
        {
            var response = new PlaneInfoResponse()
            {
                // from Gatwick
                RelativeWindX = -10.983581169243738,
                RelativeWindZ = -12.800222380537162,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(Between(slip, 40.62, 40.64));

        }

        public static bool Between(double number, double min, double max)
        {
            return number >= min && number <= max;
        }
    }
}
