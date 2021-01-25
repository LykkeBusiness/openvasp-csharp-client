using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Internals.Messages.Session
{
    public class SessionAbort
    {
        [JsonProperty("cause")]
        public string Code { get; set; }
    }
}