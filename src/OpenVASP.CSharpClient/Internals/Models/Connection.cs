using System;

namespace OpenVASP.CSharpClient.Internals
{
    public class Connection
    {
        public string Id { set; get; }
        public string InboundTopic { set; get; }
        public string OutboundTopic { set; get; }
        public string Filter { set; get; }
        public ConnectionStatus Status { set; get; }
        public string SymKeyId { get; set; }
        public string CounterPartyVaspId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SharedPrivateEncryptionKey { get; set; }
        public string PrivateKey { get; set; }
        public string CounterPartyPublicKey { get; set; }
    }

    public enum ConnectionStatus
    {
        None,
        Active,
        Passive,
        PartiallyActive
    }
}