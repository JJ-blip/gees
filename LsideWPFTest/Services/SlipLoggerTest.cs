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
                RelativeWindX = 3,
                RelativeWindZ = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(slip < 0);
        }

        [TestMethod]
        public void TestGetSlipComponents2()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on left, + slip to right
                RelativeWindX = 3,
                RelativeWindZ = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 360 - 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(slip < 0);
        }

        [TestMethod]
        public void TestGetSlipComponents3()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on right, + slip to left
                RelativeWindX = -3,
                RelativeWindZ = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(slip < 0);
        }

        [TestMethod]
        public void TestGetSlipComponents4()
        {
            var response = new PlaneInfoResponse()
            {
                // runway on left, + slip to left
                RelativeWindX = -3,
                RelativeWindZ = 4,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 0,
                GpsGroundTrueHeading = 360 - 20,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            // - 36
            Assert.IsTrue(slip < 0);
        }

        [TestMethod]
        public void TestGetSlipComponents5()
        {
            var response = new PlaneInfoResponse()
            {
                // from Gatwick
                RelativeWindX = -10.983581169243738,
                RelativeWindZ = -12.800222380537162,
                AtcRunwaySelected = true,
                AtcRunwayHeadingDegreesTrue = 257.5948486328125,
                GpsGroundTrueHeading = 266.99102914628781,
            };

            var uut = new PrivateObject(slipLogger);
            var slip = (double)uut.Invoke("GetSlipAngle", response);

            Assert.IsTrue(slip < 0);

        }

        [TestMethod]
        public void TestGetSlipComponents6()
        {
            // Runway 15deg, Wind 18 Kts from 70 deg
            // flying torwards runway on a 18 deg heading 
            var response = new PlaneInfoResponse()
            {
                AirspeedInd = 119.04691314697266,
                AltitudeAboveGround = 386.85086287943733,
                AtcRunwayAirportName = "First Flight",
                AtcRunwayHeadingDegreesTrue = 15.020002365112305,
                AtcRunwaySelected = true,
                AtcRunwayTdpointRelativePositionX = 199.38858200565153,
                AtcRunwayTdpointRelativePositionZ = -5305.04062288728,
                RelativeWindX = -12.043515216341257,
                GearPosition = 0,
                Gforce = 1.0468289824152406,
                GpsGroundTrueHeading = 19.58200100809335,
                GroundSpeed = 116.54700870895299,
                RelativeWindZ = +5.1914815506409751,
                LandingRate = 0,
                LateralSpeed = -19.2521928140399,
                Latitude = 36.002339148889696,
                LightLandingOn = false,
                Longitude = -75.67596005443454,
                OnAnyRunway = false,
                OnGround = false,
                PlaneBankDegrees = 4.8712091254213128,
                SpeedAlongHeading = 114.68570563414161,
                Type = "Beechcraft King Air 350i Asobo",
                VerticalSpeed = -27.089865803718514,
            };

            var uut = new PrivateObject(slipLogger);
            var slipLogEntry = (SlipLogEntry)uut.Invoke("GetSlipLogEntry", response);

            Assert.IsTrue(slipLogEntry.AirSpeedInd == 119);
            Assert.IsTrue(slipLogEntry.GroundSpeed == 117);
            Assert.IsTrue(slipLogEntry.Altitude == 386);
            Assert.IsTrue(slipLogEntry.Fpm == -27);

            // direction nose is pointing
            Assert.IsTrue(slipLogEntry.Heading == 19);

            // banked to the right
            Assert.IsTrue(slipLogEntry.BankAngle == 4.9);

            Assert.IsTrue(slipLogEntry.RelativeWindZ == 5.2);
            Assert.IsTrue(slipLogEntry.RelativeWindX == - 12);

            // 114 fwd spped & sideways -19 kts (to left) thus drifting left
            Assert.IsTrue(slipLogEntry.DriftAngle == -9.5);

            // RelativeWindZ 5 kts, RelativeWindX -12 (neg & to left)
            Assert.IsTrue(slipLogEntry.SlipAngle == -66.7);
        }
    }
}
