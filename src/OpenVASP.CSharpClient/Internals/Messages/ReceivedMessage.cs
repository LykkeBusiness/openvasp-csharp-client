using System;

namespace OpenVASP.CSharpClient.Internals.Messages
{
    public class ReceivedMessage
    {
        public MessageEnvelope MessageEnvelope { get; set; }
        public string Payload { get; set; }
    }
}
