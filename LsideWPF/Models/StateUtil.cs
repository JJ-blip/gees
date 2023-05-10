namespace LsideWPF.Models
{
    using System.Device.Location;

    public class StateUtil
    {
        // inputs in degrees, output meters
        public static double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var sCoord = new GeoCoordinate(latitude, longitude);
            var eCoord = new GeoCoordinate(otherLatitude, otherLongitude);

            return sCoord.GetDistanceTo(eCoord);
        }

        public static double GetFeet(double meters)
        {
            return meters * 3.280839895;
        }
    }
}
