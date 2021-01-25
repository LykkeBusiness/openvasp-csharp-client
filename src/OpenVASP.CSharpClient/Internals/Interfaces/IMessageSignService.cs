using System.Threading.Tasks;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface IMessageSignService
    {
        Task<string> SignPayloadAsync(string payload);
        bool VerifySign(string payload, string sign, string pubKey);
    }
}