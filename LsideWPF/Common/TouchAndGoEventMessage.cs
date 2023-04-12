namespace LsideWPF.Services
{
    using CommunityToolkit.Mvvm.Messaging.Messages;

    public class TouchAndGoEventMessage : ValueChangedMessage<FlightParameters>
    {
        public TouchAndGoEventMessage(FlightParameters flightParameters)
            : base(flightParameters)
        {
        }
    }
}
