using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Applications.TravelRule.Messages;
using OpenVASP.CSharpClient.Applications.TravelRule.Models;
using OpenVASP.CSharpClient.Internals.Messages;
using Xunit;

namespace OpenVASP.Tests
{
    /// <summary>
    /// In order to run given tests, populate whisperRpcUrl and ethereumRpcUrl variables in ctor and uncomment [Fact]
    /// </summary>
    public class ClientExampleTest
    {
        private readonly VaspClientSettings _settings1;
        private readonly VaspClientSettings _settings2;

        public ClientExampleTest()
        {
            var whisperRpcUrl = string.Empty;
            var ethereumRpcUrl = string.Empty;
            var indexSmartContractAddress = "0x4988a15D3CA2AEBd2Dfd02629E759492E6e29BfE";
            
            _settings1 = new VaspClientSettings
            {
                WhisperRpc = whisperRpcUrl,
                EthereumRpc = ethereumRpcUrl,
                EnvelopeMaxRetries = 2,
                EnvelopeExpirySeconds = 60,
                IndexSmartContractAddress = indexSmartContractAddress,
                LoggerFactory = null,
                PrivateMessageKey = "0x3ba7e0a15726033b13ce1e7b6fc498f888cb81c41ae92b853926513b63022d5b",
                PrivateSigningKey = "0x5d6fe7b43c2c7c7a3c65958a45b748be52d7fbd36815ff2ede2794c24f3b3334",
                PrivateTransportKey = "0x6cebbd8e7503388b11ae62aa5cf057cbd153a8f07c028a2106c3cbd1824fc516",
                VaspId = "f00000000001"
            };
            
            _settings2 = new VaspClientSettings
            {
                WhisperRpc = whisperRpcUrl,
                EthereumRpc = ethereumRpcUrl,
                EnvelopeMaxRetries = 2,
                EnvelopeExpirySeconds = 60,
                IndexSmartContractAddress = indexSmartContractAddress,
                LoggerFactory = null,
                PrivateMessageKey = "0x143a7874b569146593f775d2ac48715e11bde4515367c631609ada29f5ab25d3",
                PrivateSigningKey = "0xa9bc0610dc214b3c7da2423466fa3654a4edb34601733308df55411730c97278",
                PrivateTransportKey = "0xbdbd47e937fad99f20476a1f88eeb4b4ba3a68de23878c1dc3973a04fb25c744",
                VaspId = "f00000000002"
            };
        }

        //[Fact]
        public async Task ConnectionBetweenTwoClientsSuccessful()
        {
            var vaspClient1 = VaspClient.Create(_settings1);
            var vaspClient2 = VaspClient.Create(_settings2);
            
            var sessionRequestSemaphore = new SemaphoreSlim(0, 1);
            var sessionReplySemaphore = new SemaphoreSlim(0, 1);
            var transferRequestSemaphore = new SemaphoreSlim(0, 1);
            var transferReplySemaphore = new SemaphoreSlim(0, 1);
            var transferDispatchSemaphore = new SemaphoreSlim(0, 1);
            var transferConfirmSemaphore = new SemaphoreSlim(0, 1);
            var terminationSemaphore = new SemaphoreSlim(0, 1);

            var sessionRequestReceived = false;
            var sessionReplyReceived = false;
            var transferRequestReceived = false;
            var transferReplyReceived = false;
            var transferDispatchReceived = false;
            var transferConfirmReceived = false;
            var terminationReceived = false;

            string session1 = null;
            
            vaspClient1.OriginatorSessionApprovedEvent += async approved =>
            {
                sessionReplyReceived = true;
                sessionReplySemaphore.Release();

                var transferRequest = new TransferRequest
                {
                    Transfer = new Transfer
                    {
                        Amount = 10
                    }
                };
                
                await vaspClient1.SendApplicationMessageAsync(session1, MessageType.TransferRequest, JObject.FromObject(transferRequest));
            };
            vaspClient1.ApplicationMessageReceivedEvent += async received =>
            {
                if (received.Type == MessageType.TransferReply)
                {
                    transferReplyReceived = true;
                    transferReplySemaphore.Release();

                    var transferReply = received.Body.ToObject<TransferReply>();
                    
                    Assert.Equal(TransferReplyMessageCode.TransferAccepted, transferReply.Code);
                    
                    var transferDispatch = new TransferDispatch
                    {
                        Transfer = new Transaction
                        {
                            TransactionHash = "txhash"
                        }
                    };
                    
                    await vaspClient1.SendApplicationMessageAsync(session1, MessageType.TransferDispatch, JObject.FromObject(transferDispatch));
                }

                if (received.Type == MessageType.TransferConfirmation)
                {
                    transferConfirmReceived = true;
                    transferConfirmSemaphore.Release();

                    var transferConfirm = received.Body.ToObject<TransferConfirm>();
                    
                    Assert.Equal(TransactionConfirmCode.TransferConfirmed, transferConfirm.Code);
                    
                    await vaspClient1.SessionTerminateAsync(session1);
                }
            };

            string session2 = null;

            vaspClient2.BeneficiarySessionCreatedEvent += async created =>
            {
                sessionRequestReceived = true;
                sessionRequestSemaphore.Release();

                session2 = created.SessionId;

                await vaspClient2.SessionReplyAsync(session2, true);
            };
            vaspClient2.SessionTerminatedEvent += terminated =>
            {
                terminationReceived = true;
                terminationSemaphore.Release();

                return Task.CompletedTask;
            };
            vaspClient2.ApplicationMessageReceivedEvent += async received =>
            {
                if (received.Type == MessageType.TransferRequest)
                {
                    transferRequestReceived = true;
                    transferRequestSemaphore.Release();

                    var transferRequest = received.Body.ToObject<TransferRequest>();
                    
                    Assert.Equal(10, transferRequest.Transfer.Amount);

                    var transferReply = new TransferReply
                    {
                        Code = TransferReplyMessageCode.TransferAccepted
                    };
                    
                    await vaspClient2.SendApplicationMessageAsync(session2, MessageType.TransferReply, JObject.FromObject(transferReply));
                }

                if (received.Type == MessageType.TransferDispatch)
                {
                    transferDispatchReceived = true;
                    transferDispatchSemaphore.Release();

                    var transferDispatch = received.Body.ToObject<TransferDispatch>();
                    
                    Assert.Equal("txhash", transferDispatch.Transfer.TransactionHash);

                    var transferConfirm = new TransferConfirm
                    {
                        Code = TransactionConfirmCode.TransferConfirmed
                    };

                    await vaspClient2.SendApplicationMessageAsync(session2, MessageType.TransferConfirmation,
                        JObject.FromObject(transferConfirm));
                }
                
            };

            session1 = await vaspClient1.CreateSessionAsync(_settings2.VaspId);
            
            await Task.WhenAny(
                Task.Delay(TimeSpan.FromMinutes(2)),
                Task.WhenAll(
                    sessionRequestSemaphore.WaitAsync(),
                    sessionReplySemaphore.WaitAsync(),
                    transferRequestSemaphore.WaitAsync(),
                    transferReplySemaphore.WaitAsync(),
                    transferDispatchSemaphore.WaitAsync(),
                    transferConfirmSemaphore.WaitAsync(),
                    terminationSemaphore.WaitAsync()));

            Assert.True(sessionRequestReceived, "Session request message was not delivered");
            Assert.True(sessionReplyReceived, "Session reply message was not delivered");
            Assert.True(transferRequestReceived, "Transfer request message was not delivered");
            Assert.True(transferReplyReceived, "Transfer reply message was not delivered");
            Assert.True(transferDispatchReceived, "Transfer dispatch message was not delivered");
            Assert.True(transferConfirmReceived, "Transfer confirm message was not delivered");
            Assert.True(terminationReceived, "Termination message was not delivered");
        }
    }
}