namespace LsideWPF.Services
{
    using CommunityToolkit.Mvvm.Messaging.Messages;

    public class LandingEventMessage : ValueChangedMessage<FlightParameters>
    {
        public LandingEventMessage(FlightParameters flightParameters)
            : base(flightParameters)
        {
        }
    }
}
