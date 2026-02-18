using ShamrockRemoteAgent.ClientTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using System.Net.WebSockets;

namespace ShamrockRemoteAgent.ClientTester.Services
{
    public class BrokerWebSocketService
    {
        private ClientWebSocket? _socket;
        private CancellationTokenSource? _cts;
        public event Action? LoginHandshakeCompleted;

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
                            switch (decoded.PacketType)
                            {
                                case DataPacketTypeEnum.PING_REQ:
                                    PacketBus.PublishLog("PING_REQ received from Master");

                                    // Build empty PING_RES payload
                                    PingRes pingResPayload = new PingRes();
                                    byte[] payloadBytes = pingResPayload.Serialize(); // empty

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
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, responseBytes);

                                    await SendAsync(brokerSedingPacket);

                                    break;

                                case DataPacketTypeEnum.LOGIN_RES:
                                    PacketBus.PublishLog("LOGIN_RES received from Master");

                                    // Build empty LOGIN_ACK payload
                                    LoginAck loginAckPayload = new LoginAck();
                                    byte[] loginAckpayloadBytes = loginAckPayload.Serialize();

                                    // Build data packet
                                    var loginResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.LOGIN_ACK,
                                        PacketPayload = loginAckpayloadBytes,
                                        PacketLength = (uint)(4 + 1 + loginAckpayloadBytes.Length)
                                    };

                                    byte[] packetBytes = loginResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] brokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, packetBytes);
                                    await App.BrokerSocket.SendAsync(brokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(packetBytes);
                                    PacketBus.PublishLog($"Sent LoginAck successfully");
                                    LoginHandshakeCompleted?.Invoke();

                                    break;

                                case DataPacketTypeEnum.CLI_CON_REQ:
                                    PacketBus.PublishLog("CLI_CON_REQ received from Master");
                                    ClientConnectRes cliConResPayload = new ClientConnectRes();
                                    cliConResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] cliConnResPayloadBytes = cliConResPayload.Serialize();

                                    // Build data packet
                                    var cliConResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.CLI_CON_RES,
                                        PacketPayload = cliConnResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + cliConnResPayloadBytes.Length)
                                    };

                                    byte[] cliConnResPacketBytes = cliConResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] cliConnResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, cliConnResPacketBytes);

                                    await App.BrokerSocket.SendAsync(cliConnResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(cliConnResPacketBytes);
                                    PacketBus.PublishLog($"Sent CLI_CON_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.CLI_DISCON_REQ:
                                    PacketBus.PublishLog("CLI_DISCON_REQ received from Master");
                                    ClientDisconnectRes cliDisconResPayload = new ClientDisconnectRes();
                                    cliDisconResPayload.ResultCode.FieldData= RPErrorCodeEnum.NO_ERRORS;
                                    byte[] cliDisconResPayloadBytes = cliDisconResPayload.Serialize();

                                    // Build data packet
                                    var cliDisconResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.CLI_DISCON_RES,
                                        PacketPayload = cliDisconResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + cliDisconResPayloadBytes.Length)
                                    };

                                    byte[] cliDisconResPacketBytes = cliDisconResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] cliDisconResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, cliDisconResPacketBytes);
                                    await App.BrokerSocket.SendAsync(cliDisconResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(cliDisconResPacketBytes);
                                    PacketBus.PublishLog($"Sent CLI_DISCON_RES successfully!");

                                    break;
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
