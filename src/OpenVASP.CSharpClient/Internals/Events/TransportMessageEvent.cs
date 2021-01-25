using System;

namespace OpenVASP.CSharpClient.Internals.Events
{
    public class TransportMessageEvent
    {
        public string ConnectionId { set; get; }

        public string SenderVaspId { get; set; }

        public string ReceiverVaspId { get; set; }

        public Instruction Instruction { get; set; }

        public string Payload { set; get; }

        public DateTime Timestamp { get; set; }

        public string SigningKey { get; set; }
    }
}