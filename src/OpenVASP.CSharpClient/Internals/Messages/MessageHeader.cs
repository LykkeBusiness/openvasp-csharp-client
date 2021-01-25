using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Internals.Messages
{
    public class MessageHeader
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("sender")]
        public string SenderVaspId { get; set; }

        [JsonProperty("receiver")]
        public string ReceiverVaspId { get; set; }

        [JsonProperty("msgid")]
        public string MessageId { get; set; }

        [JsonProperty("session")]
        public string SessionId { get; set; }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public MessageType MessageType { get; set; }
        
        [JsonProperty("ecdhpk")]
        public string EcdhPk { set; get; }

        public MessageHeader()
        {
        }

        public MessageHeader(
            string senderVaspId,
            string receiverVaspId,
            string messageId,
            string sessionId,
            MessageType messageType,
            string ecdhPk)
        {
            Version = "1.0";
            SenderVaspId = senderVaspId;
            ReceiverVaspId = receiverVaspId;
            MessageId = messageId;
            SessionId = sessionId;
            MessageType = messageType;
            EcdhPk = ecdhPk;
        }
    }
}