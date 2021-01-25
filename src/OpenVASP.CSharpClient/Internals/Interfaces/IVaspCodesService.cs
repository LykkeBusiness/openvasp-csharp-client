using System.Threading.Tasks;

namespace OpenVASP.CSharpClient.Internals.Interfaces
{
    public interface IVaspCodesService
    {
        Task<string> GetTransportKeyAsync(string vaspCode);
        Task<string> GetSigningKeyAsync(string vaspCode);
        Task<string> GetMessageKeyAsync(string vaspCode);
    }
}