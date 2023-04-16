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
                CrossWind = 3,
                HeadWind = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 20,
            };

            var uut = new PrivateObject(slipLogger);
            var output = (double)uut.Invoke("GetSlipAngle", response);
            Assert.IsTrue(output > 0);
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

            // +9.36
            var clo = (double)uut.Invoke("GetCenterLineOffsetDegrees", response);

            Assert.IsTrue((clo - 9.36) < 0.1);
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

            Assert.IsTrue((clo + 20) < 0.1);
        }

        [TestMethod]
        public void TestGetSlipComponents()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on right, + slip to right
                CrossWind = 3,
                HeadWind = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            var (sideSlip, forwardSlip) =  ((double, double)) uut.Invoke("GetSlipComponents", response);
            
            // 20.0
            Assert.IsTrue(sideSlip > 0);

            // 16.8
            Assert.IsTrue(forwardSlip > 0);

            // 0.0
            Assert.IsTrue(slip - (sideSlip + forwardSlip) < 0.1);
        }

        [TestMethod]
        public void TestGetSlipComponents2()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on left, + slip to right
                CrossWind = 3,
                HeadWind = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 360 - 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            var (sideSlip, forwardSlip) = ((double, double))uut.Invoke("GetSlipComponents", response);
            
            // 20.0
            Assert.IsTrue(sideSlip > 0);

            // 16.8
            Assert.IsTrue(forwardSlip > 0);

            // 0.0
            Assert.IsTrue(slip - (sideSlip + forwardSlip) < 0.01);
        }

        [TestMethod]
        public void TestGetSlipComponents3()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on right, + slip to left
                CrossWind = -3,
                HeadWind = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            // -36.
            Assert.IsTrue(slip < 0);

            var (sideSlip, forwardSlip) = ((double, double))uut.Invoke("GetSlipComponents", response);

            // -20.0
            Assert.IsTrue(sideSlip < 0);

            // -16.8
            Assert.IsTrue(forwardSlip < 0);

            // 0.0
            Assert.IsTrue(slip - (sideSlip + forwardSlip) < 0.1);
        }

        [TestMethod]
        public void TestGetSlipComponents4()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on left, + slip to left
                CrossWind = -3,
                HeadWind = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 360 - 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            // - 36
            Assert.IsTrue(slip < 0);

            var (sideSlip, forwardSlip) = ((double, double))uut.Invoke("GetSlipComponents", response);

            // -20.0
            Assert.IsTrue(sideSlip < 0);

            // -16.8
            Assert.IsTrue(forwardSlip < 0);

            // 0.0
            Assert.IsTrue(slip - (sideSlip + forwardSlip) < 0.1);
        }

        [TestMethod]
        public void TestGetSlipComponents5()
        {
            var response = new PlaneInfoResponse()
            {
                // from Gatwick
                CrossWind = -10.983581169243738,
                HeadWind = -12.800222380537162,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 257.5948486328125,
                GpsGroundTrueHeading = 266.99102914628781,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            var (sideSlip, forwardSlip) = ((double, double))uut.Invoke("GetSlipComponents", response);

            // +9.36
            Assert.IsTrue(sideSlip > 9.3);

            // 31.23
            Assert.IsTrue(forwardSlip > 31.2);

            // 0.0
            Assert.IsTrue(slip - (sideSlip + forwardSlip) < 0.1);
        }
    }
}
