using Newtonsoft.Json;

namespace OpenVASP.CSharpClient.Internals.Messages.Session
{
    public class SessionReply
    {
        [JsonProperty("return")]
        public string Code { get; set; }
    }
}