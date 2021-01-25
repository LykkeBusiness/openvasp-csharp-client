using System.Collections.Generic;
using System.Threading.Tasks;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Models;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface IWhisperService
    {
        Task<string> RegisterSymKeyAsync(string privateKeyHex);
        Task<string> RegisterKeyPairAsync(string privateKeyHex);
        Task<string> CreateMessageFilterAsync(string topicHex, string privateKeyId = null, 
            string symKeyId = null, string signingKey = null);
        Task<string> SendMessageAsync(
            string topic, 
            string encryptionKey, 
            EncryptionType encryptionType,
            string payload);
        Task<IReadOnlyCollection<ReceivedMessage>> GetMessagesAsync(string messageFilter);
    }
}