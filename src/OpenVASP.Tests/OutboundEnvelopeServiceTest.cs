using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenVASP.CSharpClient.Internals;
using OpenVASP.CSharpClient.Internals.Interfaces;
using OpenVASP.CSharpClient.Internals.Messages;
using OpenVASP.CSharpClient.Internals.Models;
using OpenVASP.CSharpClient.Internals.Services;
using Xunit;

namespace OpenVASP.Tests
{
    public class OutboundEnvelopeServiceTest
    {
        [Fact]
        public async Task MessageNotAckedAsync()
        {
            bool eventTriggered = false;
            
            var (service, whisperPrcStub) = CreateService(0.1, 2);
            
            service.OutboundEnvelopeReachedMaxResends += e =>
            {
                eventTriggered = true;
                return Task.CompletedTask;
            };

            await service.SendEnvelopeAsync(CreateEnvelope(), true);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            whisperPrcStub.Verify(x => x.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EncryptionType>(),
                It.IsAny<string>()), Times.Exactly(3));

            Assert.True(eventTriggered);
        }
        
        [Fact]
        public async Task MessageAckedRightAwayAsync()
        {
            bool eventTriggered = false;
            
            var (service, whisperPrcStub) = CreateService(0.1, 2);

            service.OutboundEnvelopeReachedMaxResends += e =>
            {
                eventTriggered = true;
                return Task.CompletedTask;
            };
            
            var envelope = CreateEnvelope();
            
            await service.SendEnvelopeAsync(envelope, true);

            await Task.Delay(TimeSpan.FromSeconds(0.08));

            await service.RemoveQueuedEnvelopeAsync(envelope.Id);
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            whisperPrcStub.Verify(x => x.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EncryptionType>(),
                It.IsAny<string>()), Times.Once);
            
            Assert.False(eventTriggered);
        }

        private OutboundEnvelope CreateEnvelope()
        {
            return new OutboundEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Envelope = new MessageEnvelope()
            };
        }

        private (IOutboundEnvelopeService, Mock<IWhisperService>) CreateService(double expiry, int retries)
        {
            var whisperRpcStub = new Mock<IWhisperService>();
            whisperRpcStub.Setup(x => x.SendMessageAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EncryptionType>(),
                    It.IsAny<string>()))
                .ReturnsAsync(Guid.NewGuid().ToString("N"));
            
            return (new OutboundEnvelopeService(whisperRpcStub.Object, expiry, retries, null), whisperRpcStub);
        }
    }
}