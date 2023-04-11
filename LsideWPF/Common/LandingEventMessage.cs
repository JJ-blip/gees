using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LsideWPF.Common
{
    public class LandingEventMessage : ValueChangedMessage<FlightParameters>
    {
        public LandingEventMessage(FlightParameters flightParameters) : base(flightParameters)
        {
        }
    }
}
