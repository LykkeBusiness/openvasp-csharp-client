using System;
using System.Threading.Tasks;
using OpenVASP.CSharpClient.Internals.Events;
using OpenVASP.CSharpClient.Internals.Messages;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface IOutboundEnvelopeService
    {
        event Func<OutboundEnvelopeReachedMaxResendsEvent, Task> OutboundEnvelopeReachedMaxResends;
        Task SendEnvelopeAsync(OutboundEnvelope outboundEnvelope, bool doRetries);
        Task AcknowledgeAsync(MessageEnvelope messageEnvelope, string payload);
        Task RemoveQueuedEnvelopeAsync(string payloadEnvelopeAck);
    }
}