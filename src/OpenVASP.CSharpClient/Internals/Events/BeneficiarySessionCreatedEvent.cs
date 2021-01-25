namespace OpenVASP.CSharpClient.Internals.Events
{
    public class BeneficiarySessionCreatedEvent : SessionEventBase
    {
        public string CounterPartyVaspId { set; get; }
    }
}