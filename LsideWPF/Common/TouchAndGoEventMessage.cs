using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LsideWPF.Common
{
    public class TouchAndGoEventMessage : ValueChangedMessage<FlightParameters>
    {
        public TouchAndGoEventMessage(FlightParameters flightParameters) : base(flightParameters)
        {

        }
    }
}
