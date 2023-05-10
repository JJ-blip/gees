namespace LsideWPF.Common
{
    using CommunityToolkit.Mvvm.Messaging.Messages;
    using LsideWPF.Models;

    public class TouchAndGoEventMessage : ValueChangedMessage<LogEntry>
    {
        public TouchAndGoEventMessage(LogEntry logEntry)
            : base(logEntry)
        {
        }
    }
}
