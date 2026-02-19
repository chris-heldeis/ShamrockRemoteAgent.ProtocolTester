using ShamrockRemoteAgent.MasterTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginRes;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage;
using System.Net.WebSockets;

namespace ShamrockRemoteAgent.MasterTester.Services;

public class BrokerWebSocketService
{
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _cts;

    public bool IsConnected =>
        _socket != null && _socket.State == WebSocketState.Open;

    public async Task ConnectAsync(string host, int port, string masterId)
    {
        if (IsConnected)
            return;

        _socket = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        var uri = new Uri(
            $"ws://{host}:{port}/?role=master&id={masterId}");

        await _socket.ConnectAsync(uri, CancellationToken.None);

        PacketBus.PublishLog(
            $"Connected to WebSocket server {host}:{port} as {masterId}");
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
                    if (brokerRecivingPacket.Payload.Length > 0) { 
                        var decoded = PacketDecoder.Decode(brokerRecivingPacket.Payload);
                        PacketBus.Publish(brokerRecivingPacket.Payload);

                        PacketBus.PublishLog(
                            $"Received {brokerRecivingPacket.Payload.Length} bytes");
                        PacketBus.PublishDecoded(decoded);

                        // Sent Response
                        switch (decoded.PacketType)
                        {
                            case DataPacketTypeEnum.PING_RES:
                                PacketBus.PublishLog("PING_RES received from Client");
                                break;

                            case DataPacketTypeEnum.LOGIN_REQ:
                                PacketBus.PublishLog("LOGIN_REQ received from Client");

                                // Send LoginRes
                                LoginRes loginResPayload = new LoginRes();
                                loginResPayload.ResultCode.FieldData = LoginErrorCodeEnum.NO_ERRORS;
                                byte[] loginResPayloadBytes = loginResPayload.Serialize();

                                // Build data packet
                                var loginResPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.LOGIN_RES,
                                    PacketPayload = loginResPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + loginResPayloadBytes.Length)
                                };

                                byte[] loginResPacketBytes = loginResPacket.Serialize();
                                // Wrap with Broker protocol
                                byte[] loginResBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, loginResPacketBytes);

                                await App.BrokerSocket.SendAsync(loginResBrokerPacket);
                                // Publish to HexViewer
                                PacketBus.Publish(loginResPacketBytes);
                                PacketBus.PublishLog($"Sent LoginRes successfully!");

                                break;

                            case DataPacketTypeEnum.CLI_CON_RES:
                                PacketBus.PublishLog("CLI_CON_RES received from Client");
                                ClientConnectAck cliConAckPayload = new ClientConnectAck();
                                byte[] cliConAckpayloadBytes = cliConAckPayload.Serialize();

                                // Build data packet
                                var cliConAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.CLI_CON_ACK,
                                    PacketPayload = cliConAckpayloadBytes,
                                    PacketLength = (uint)(4 + 1 + cliConAckpayloadBytes.Length)
                                };

                                byte[] cliConAckPacketBytes = cliConAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] cliConAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, cliConAckPacketBytes);
                                await App.BrokerSocket.SendAsync(cliConAckBrokerPacket);
                                PacketBus.Publish(cliConAckPacketBytes);
                                PacketBus.PublishLog($"Sent CLI_CON_ACK successfully");

                                break;

                            case DataPacketTypeEnum.CLI_DISCON_RES:
                                PacketBus.PublishLog("CLI_DISCON_RES received from Client");
                                ClientDisconnectAck cliDisconAckPayload = new ClientDisconnectAck();
                                byte[] cliDisconAckpayloadBytes = cliDisconAckPayload.Serialize();

                                // Build data packet
                                var cliDisconAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.CLI_DISCON_ACK,
                                    PacketPayload = cliDisconAckpayloadBytes,
                                    PacketLength = (uint)(4 + 1 + cliDisconAckpayloadBytes.Length)
                                };

                                byte[] cliDisconAckPacketBytes = cliDisconAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] cliDisconAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, cliDisconAckPacketBytes);
                                await App.BrokerSocket.SendAsync(cliDisconAckBrokerPacket);
                                PacketBus.Publish(cliDisconAckPacketBytes);
                                PacketBus.PublishLog($"Sent CLI_DISCON_ACK successfully");

                                break;

                            case DataPacketTypeEnum.TX_FRAME_RES:
                                PacketBus.PublishLog("TX_FRAME_RES received from Client");
                                SendMessageAck txFrameAckPayload = new SendMessageAck();
                                byte[] txFrameAckPayloadBytes = txFrameAckPayload.Serialize();

                                // Build data packet
                                var txFrameAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.TX_FRAME_ACK,
                                    PacketPayload = txFrameAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + txFrameAckPayloadBytes.Length)
                                };

                                byte[] txFrameAckPacketBytes = txFrameAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] txFrameAckBrokerPacket = 
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, txFrameAckPacketBytes);
                                await App.BrokerSocket.SendAsync(txFrameAckBrokerPacket);
                                PacketBus.Publish(txFrameAckPacketBytes);
                                PacketBus.PublishLog($"Sent TX_FRAME_ACK successfully");

                                break;

                            case DataPacketTypeEnum.RX_FRAME_RES:
                                PacketBus.PublishLog("RX_FRAME_RES received from Client");
                                ReadMessageAck rxFrameAckPayload = new ReadMessageAck();
                                byte[] rxFrameAckPayloadBytes = rxFrameAckPayload.Serialize();

                                // Build data packet
                                var rxFrameAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.RX_FRAME_ACK,
                                    PacketPayload = rxFrameAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + rxFrameAckPayloadBytes.Length)
                                };

                                byte[] rxFrameAckPacketBytes = rxFrameAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] rxFrameAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, rxFrameAckPacketBytes);
                                await App.BrokerSocket.SendAsync(rxFrameAckBrokerPacket);
                                PacketBus.Publish(rxFrameAckPacketBytes);
                                PacketBus.PublishLog($"Sent RX_FRAME_ACK successfully");

                                break;

                            default:
                                break;
                        }
                    } 
                    else
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
            _cts?.Cancel(); // ✅ stop receive loop
            if (_socket.State == WebSocketState.Open)
            {
                PacketBus.PublishLog("Closing WebSocket connection...");
                await _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Master shutting down",
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
