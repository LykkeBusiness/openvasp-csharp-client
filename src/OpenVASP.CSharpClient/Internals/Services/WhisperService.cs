using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Shh.DTOs;
using Nethereum.Web3;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Models;

namespace OpenVASP.CSharpClient.Internals.Services
{
    internal class WhisperService : IWhisperService
    {
        private readonly IWeb3 _web3;
        private readonly ILogger<WhisperService> _logger;

        public WhisperService(IWeb3 web3, ILoggerFactory loggerFactory)
        {
            _web3 = web3;
            _logger = loggerFactory?.CreateLogger<WhisperService>();
        }

        public async Task<string> RegisterSymKeyAsync(string privateKey)
        {
            var symKeyId = await _web3.Shh.SymKey.AddSymKey.SendRequestAsync(privateKey);

            return symKeyId;
        }

        public async Task<string> RegisterKeyPairAsync(string privateKey)
        {
            var keyPairId = await _web3.Shh.KeyPair.AddPrivateKey.SendRequestAsync(privateKey);

            return keyPairId;
        }

        public async Task<string> CreateMessageFilterAsync(string topic, string privateKeyId = null, string symKeyId = null, string signingKey = null)
        {
            var filter = await _web3.Shh.MessageFilter.NewMessageFilter.SendRequestAsync(new MessageFilterInput()
            {
                Topics = new Object [] {topic.EnsureHexPrefix()},
                PrivateKeyID = privateKeyId,
                SymKeyID = symKeyId,

            });
            
            return filter;
        }

        public async Task<string> SendMessageAsync(string topic, string encryptionKey, 
            EncryptionType encryptionType, string payload)
        {
            var messageInput = new MessageInput()
            {
                Topic = topic.EnsureHexPrefix(),
                Payload = payload,
                //TODO: Find a way to calculate it
                PowTime = 12,
                PowTarget = 0.4,
                Ttl = 300,
            };

            switch (encryptionType)
            {
                case EncryptionType.Asymmetric:
                    messageInput.PubKey = encryptionKey;
                    break;
                case EncryptionType.Symmetric:
                    messageInput.SymKeyID = encryptionKey;
                    break;
                default:
                    throw new ArgumentException(
                        $"Current Encryption type {encryptionType} is not supported.",
                        nameof(encryptionType));
            }

            var messageHash = await _web3.Shh.Post.SendRequestAsync(messageInput);

            return messageHash;
        }

        public async Task<IReadOnlyCollection<ReceivedMessage>> GetMessagesAsync(string source)
        {
            try
            {
                var messages = await _web3.Shh.MessageFilter.GetFilterMessages.SendRequestAsync(source);

                if (messages == null || messages.Length == 0)
                {
                    return new ReceivedMessage[] { };
                }

                var receivedMessages = messages.Select(x => new ReceivedMessage
                {
                    MessageEnvelope = new MessageEnvelope
                    {
                        Topic = x.Topic,
                        EncryptionType = EncryptionType.Asymmetric,
                        EncryptionKey = x.RecipientPublicKey,
                        SigningKey = x.Sig
                    },
                    Payload = x.Payload
                }).ToArray();

                return receivedMessages;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Failed to get new messages");
                return new ReceivedMessage[]{};
            }
        }
    }
}