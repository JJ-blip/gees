namespace LsideWPF.Common
{
    using CommunityToolkit.Mvvm.Messaging.Messages;
    using LsideWPF.Models;

    public class LandingEventMessage : ValueChangedMessage<LogEntry>
    {
        public LandingEventMessage(LogEntry logEntry)
            : base(logEntry)
        {
        }
    }
}
