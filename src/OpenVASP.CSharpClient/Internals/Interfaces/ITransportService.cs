using System;
using System.Threading.Tasks;
using OpenVASP.CSharpClient.Internals.Events;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface ITransportService : IDisposable
    {
        event Func<TransportMessageEvent, Task> TransportMessageReceived;
        Task<string> CreateConnectionAsync(string counterPartyVaspId);
        Task SendAsync(string connectionId, string message, Instruction instruction, string receiverVaspId);
    }
}