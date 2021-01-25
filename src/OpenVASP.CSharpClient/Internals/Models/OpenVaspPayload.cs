using System;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenVASP.CSharpClient.Internals
{
    public class OpenVaspPayloadBase
    {
        protected string _envelopeId;
        
        public Instruction Instruction { get; protected set; }
        public string EnvelopeId // 16 bytes, 32 hex chars
        {
            get => _envelopeId;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.Length != 32)
                    throw new ArgumentException($"{nameof(EnvelopeId)} must have length 32");
                _envelopeId = value;
            }
        }
    }
    
    public class OpenVaspPayload : OpenVaspPayloadBase
    {
        private string _connectionId;
        private string _senderVaspId;
        private string _envelopeAck;
        private string _returnTopic;
        private string _ecdhPk;
        
        //always present
        public byte Version { get; private set; }
        public byte Flags { get; private set; }
        public string SenderVaspId // 6 bytes, 12 hex chars
        {
            get => _senderVaspId;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.Length != 12)
                    throw new ArgumentException($"{nameof(SenderVaspId)} must have length 12");
                _senderVaspId = value;
            }
        }
        public string ConnectionId // 16 bytes, 32 hex chars
        {
            get => _connectionId;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.Length != 32)
                    throw new ArgumentException($"{nameof(ConnectionId)} must have length 32");
                _connectionId = value;
            }
        }

        // optional
        public string EnvelopeAck // 16 bytes, 32 hex chars
        {
            get => _envelopeAck;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.Length != 32)
                    throw new ArgumentException($"{nameof(EnvelopeAck)} must have length 32");
                _envelopeAck = value;
            }
        }
        public string ReturnTopic // 4 bytes, 8 hex chars
        {
            get => _returnTopic == null ? null : $"0x{_returnTopic}";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.StartsWith("0x"))
                    value = value.Substring(2);
                if (value.Length != 8)
                    throw new ArgumentException($"{nameof(ReturnTopic)} must have length 8");
                _returnTopic = value;
            }
        }
        public string EcdhPk // 32 bytes, 64 hex chars
        {
            get => _ecdhPk;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                if (value.StartsWith("0x"))
                    value = value.Substring(2);
                if (value.Length != 66)
                    throw new ArgumentException($"{nameof(EcdhPk)} must have length 66");
                _ecdhPk = value;
            }
        }
        public string OvMessage { get; set; } // arbitrary size

        public OpenVaspPayload(
            Instruction instruction,
            string senderVaspId,
            string connectionId,
            string envelopeId)
        {
            Instruction = instruction;
            SenderVaspId = senderVaspId;
            ConnectionId = connectionId;
            EnvelopeId = envelopeId;
        }

        private OpenVaspPayload()
        {
        }

        public static OpenVaspPayload Create(string payload)
        {
            var result = new OpenVaspPayload();

            // always present
            var first2bytes = payload.Substring(2, 4).HexToByteArray(); //skip hex prefix
            result.Version = first2bytes[0];
            byte instructionAndFlagsByte = first2bytes[1];
            result.Instruction = (Instruction)(instructionAndFlagsByte >> 5);
            result.Flags = (byte)(instructionAndFlagsByte % 32);

            result.SenderVaspId = payload.Substring(6, 12); // start - hex prefix + 2 bytes * 2 chars, 6 * 2 chars long
            result.ConnectionId = payload.Substring(18, 32); //start - hex prefix + 8 bytes * 2 chars, 16 * 2 chars long
            result.EnvelopeId = payload.Substring(50, 32); // start - hex prefix + 24 bytes * 2 chars, 16 * 2 chars long

            // optional
            if (result.Instruction == Instruction.Ack)
            {
                result.EnvelopeAck = payload.Substring(82, 32); //start - hex prefix + 40 bytes * 2 chars, 16 * 2 chars long
                return result;
            }

            if (result.Instruction != Instruction.Deny)
            {
                result.ReturnTopic = payload.Substring(82, 8); // start - hex prefix + 40 bytes * 2 chars, 4 * 2 chars long
            }

            if (result.Instruction == Instruction.Invite || result.Instruction == Instruction.Accept)
            {
                result.EcdhPk = payload.Substring(90, 66); // start - hex prefix + 44 bytes * 2 chars, 33 * 2 chars long  
            }

            var headerBytesLength = result.Instruction switch
            {
                Instruction.Invite => 156,
                Instruction.Accept => 156,
                Instruction.Deny => 82,
                _ => 90
            };

            var ovMessage = payload.Substring(headerBytesLength);

            result.OvMessage = ovMessage.HexToUTF8String().IsValidJson() ? ovMessage.HexToUTF8String() : ovMessage;

            return result;
        }

        public override string ToString()
        {
            // always present
            byte instructionAndFlagsByte = (byte)(((byte)Instruction) * 32); // 2^5
            if (Flags > 32)
                throw new InvalidOperationException($"{nameof(Flags)} have invalid value");
            instructionAndFlagsByte += Flags;
            var prefixBytes = new byte[2] { Version, instructionAndFlagsByte };
            var prefixStr = prefixBytes.ToHex(false);
            var header = $"0x{prefixStr}{_senderVaspId}{_connectionId}{_envelopeId}";

            // optional
            if (Instruction == Instruction.Ack)
            {
                header += _envelopeAck ?? throw new InvalidOperationException($"{nameof(EnvelopeAck)} must be set");
                return header;
            }

            if (Instruction != Instruction.Deny)
            {
                header += _returnTopic ?? throw new InvalidOperationException($"{nameof(ReturnTopic)} must be set");
            }

            if (Instruction == Instruction.Invite || Instruction == Instruction.Accept)
            {
                header += _ecdhPk ?? throw new InvalidOperationException($"{nameof(EcdhPk)} must be set");
            }

            var hexBody = OvMessage.IsHex() ? OvMessage : OvMessage?.ToHexUTF8().Substring(2); // removing hex prefix
            return $"{header}{hexBody}"; // header is already in hex
        }
    }
    
    public static class HexHelper
    {
        public static bool IsHex(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) && System.Text.RegularExpressions.Regex.IsMatch(str, "^[0-9a-fA-F]+$");
        }
        
        public static bool IsValidJson(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return false;}
            str = str.Trim();
            if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
                (str.StartsWith("[") && str.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(str);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}