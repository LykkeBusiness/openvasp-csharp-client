using System;
using OpenVASP.CSharpClient.Internals.Messages;

namespace OpenVASP.CSharpClient.Internals
{
    public class OutboundEnvelope
    {
        public string Id { get; set; }

        public MessageEnvelope Envelope { get; set; }

        public int TotalResents { get; set; }

        public string ConnectionId { get; set; }

        public string Payload { get; set; }
    }
}