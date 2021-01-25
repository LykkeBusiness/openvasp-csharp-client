using System;

namespace OpenVASP.CSharpClient.Internals.Models
{
    public class Session
    {
        public string Id { set; get; }
        public SessionType Type { get; set; }
        public string CounterPartyVaspId { get; set; }
        public SessionState State { set; get; }
        public string ConnectionId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string EcdhPrivateKey { set; get; }
        public string TempAesMessageKey { set; get; }
        public string EstablishedAesMessageKey { set; get; }
    }

    public enum SessionType
    {
        Originator,
        Beneficiary
    }
    
    public enum SessionState
    {
        Created,
        Initiated,
        Invited,
        Declined,
        Open,
        Closed,
        Aborted
    }
}