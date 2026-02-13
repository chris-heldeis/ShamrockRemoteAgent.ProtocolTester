using ShamrockRemoteAgent.ClientTester.Helpers;
using ShamrockRemoteAgent.ClientTester.Models;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using System.Net.WebSockets;
using System.Text;

namespace ShamrockRemoteAgent.ClientTester.Services
{
    public class BrokerWebSocketService
    {
        private ClientWebSocket? _socket;
        private CancellationTokenSource? _cts;

        public bool IsConnected =>
            _socket != null && _socket.State == WebSocketState.Open;

        public async Task ConnectAsync(string host, int port, string clientId)
        {
            if (IsConnected)
                return;

            _socket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            var uri = new Uri(
                $"ws://{host}:{port}/?role=client&id={clientId}");

            await _socket.ConnectAsync(uri, CancellationToken.None);

            PacketBus.PublishLog(
                $"Connected to WebSocket server {host}:{port} as {clientId}");
            _ = Task.Run(ReceiveLoop);
        }

        private async Task ReceiveLoop()
        {
            if (_socket == null || _cts == null)
                return;

            var buffer = new byte[8192];

            try
            {
                while (_socket.State == WebSocketState.Open)
                {
                    var result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        PacketBus.PublishLog("Broker closed the connection");
                        break;
                    }

                    var receivedBytes = buffer[..result.Count];


                    // decode packet
                    try
                    {
                        var brokerRecivingPacket = BrokerProtocol.Decode(receivedBytes);
                        if (brokerRecivingPacket.Payload.Length > 0)
                        {
                            var decoded = PacketDecoder.Decode(brokerRecivingPacket.Payload);
                            PacketBus.Publish(brokerRecivingPacket.Payload);
                            PacketBus.PublishDecoded(decoded);

                            PacketBus.PublishLog(
                                $"Received {brokerRecivingPacket.Payload.Length} bytes");

                            // Sent Response
                            if (decoded.PacketType == DataPacketTypeEnum.PING_REQ)
                            {
                                PacketBus.PublishLog("PING_REQ received from Master");

                                // Build empty PING_RES payload
                                var payload = new PingRes();
                                byte[] payloadBytes = payload.Serialize(); // empty

                                var responsePacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.PING_RES,
                                    PacketPayload = payloadBytes,
                                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                                };

                                byte[] responseBytes = responsePacket.Serialize();

                                // Show outgoing response
                                PacketBus.Publish(responseBytes);
                                PacketBus.PublishLog("Sending PING_RES to Master");

                                // Wrap with Broker protocol
                                byte[] brokerSedingPacket =
                                    BrokerProtocol.Encode(BrokerPacketType.COM_DATA, responseBytes);

                                await SendAsync(brokerSedingPacket);
                            }
                        } else
                        {
                            PacketBus.Publish(receivedBytes);
                            PacketBus.PublishBrokerDecoded(brokerRecivingPacket);

                            PacketBus.PublishLog(
                                $"Received {receivedBytes.Length} bytes");
                        }

                    }
                    catch (Exception ex)
                    {
                        PacketBus.PublishLog($"Decode error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"WebSocket receive error: {ex.Message}");
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (!IsConnected || _socket == null)
                throw new InvalidOperationException("WebSocket not connected");

            await _socket.SendAsync(
                data,
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None);

            PacketBus.PublishLog("Request sent successfully");
        }

        public async Task CloseAsync()
        {
            if (_socket == null)
                return;

            try
            {
                _cts?.Cancel();
                if (_socket.State == WebSocketState.Open)
                {
                    PacketBus.PublishLog("Closing WebSocket connection...");
                    await _socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client shutting down",
                        CancellationToken.None);
                }

                _socket.Dispose();
                _socket = null;

                PacketBus.PublishLog("WebSocket closed");
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"Error while closing WebSocket: {ex.Message}");
            }
            finally
            {
                _socket?.Dispose();
                _socket = null;
                _cts = null;

                PacketBus.PublishLog("WebSocket closed");
            }
        }
    }
}
