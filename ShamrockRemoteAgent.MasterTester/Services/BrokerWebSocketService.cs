using ShamrockRemoteAgent.MasterTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginRes;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Close;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetHardwareStatus;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetLastErrorMsg;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadDetailedVersion;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadVersion;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand;
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

                            case DataPacketTypeEnum.TX_CMD_RES:
                                PacketBus.PublishLog("TX_CMD_RES received from Client");
                                SendCommandAck txCmdAckPayload = new SendCommandAck();
                                byte[] txCmdAckPayloadBytes = txCmdAckPayload.Serialize();

                                // Build data packet
                                var txCmdAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.TX_CMD_ACK,
                                    PacketPayload = txCmdAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + txCmdAckPayloadBytes.Length)
                                };

                                byte[] txCmdAckPacketBytes = txCmdAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] txCmdAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, txCmdAckPacketBytes);
                                await App.BrokerSocket.SendAsync(txCmdAckBrokerPacket);
                                PacketBus.Publish(txCmdAckPacketBytes);
                                PacketBus.PublishLog($"Sent TX_CMD_ACK successfully");

                                break;

                            case DataPacketTypeEnum.READ_VER_RES:
                                PacketBus.PublishLog("READ_VER_RES received from Client");
                                ReadVersionAck readVerAckPayload = new ReadVersionAck();
                                byte[] readVerAckPayloadBytes = readVerAckPayload.Serialize();

                                // Build data packet
                                var readVerAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.READ_VER_ACK,
                                    PacketPayload = readVerAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + readVerAckPayloadBytes.Length)
                                };

                                byte[] readVerAckPacketBytes = readVerAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] readVerAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, readVerAckPacketBytes);
                                await App.BrokerSocket.SendAsync(readVerAckBrokerPacket);
                                PacketBus.Publish(readVerAckPacketBytes);
                                PacketBus.PublishLog($"Sent READ_VER_ACK successfully");

                                break;

                            case DataPacketTypeEnum.GET_ERR_MSG_RES:
                                PacketBus.PublishLog("GET_ERR_MSG_RES received from Client");
                                GetErrorMsgAck errMsgAckPayload = new GetErrorMsgAck();
                                byte[] errMsgAckPayloadBytes = errMsgAckPayload.Serialize();

                                // Build data packet
                                var errMsgAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.GET_ERR_MSG_ACK,
                                    PacketPayload = errMsgAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + errMsgAckPayloadBytes.Length)
                                };

                                byte[] errMsgAckPacketBytes = errMsgAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] errMsgAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, errMsgAckPacketBytes);
                                await App.BrokerSocket.SendAsync(errMsgAckBrokerPacket);
                                PacketBus.Publish(errMsgAckPacketBytes);
                                PacketBus.PublishLog($"Sent GET_ERR_MSG_ACK successfully");

                                break;

                            case DataPacketTypeEnum.GET_HW_STATUS_RES:
                                PacketBus.PublishLog("GET_HW_STATUS_RES received from Client");
                                GetHardwareStatusAck getHWAckPayload = new GetHardwareStatusAck();
                                byte[] getHWAckPayloadBytes = getHWAckPayload.Serialize();

                                // Build data packet
                                var getHWAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.GET_HW_STATUS_ACK,
                                    PacketPayload = getHWAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + getHWAckPayloadBytes.Length)
                                };

                                byte[] getHWAckPacketBytes = getHWAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] getHWAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, getHWAckPacketBytes);
                                await App.BrokerSocket.SendAsync(getHWAckBrokerPacket);
                                PacketBus.Publish(getHWAckPacketBytes);
                                PacketBus.PublishLog($"Sent GET_HW_STATUS_ACK successfully");

                                break;

                            case DataPacketTypeEnum.GET_LAST_ERR_MSG_RES:
                                PacketBus.PublishLog("GET_LAST_ERR_MSG_RES received from Client");
                                GetLastErrorMsgAck lastErrAckPayload = new GetLastErrorMsgAck();
                                byte[] lastErrAckPayloadBytes = lastErrAckPayload.Serialize();

                                // Build data packet
                                var lastErrAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.GET_LAST_ERR_MSG_ACK,
                                    PacketPayload = lastErrAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + lastErrAckPayloadBytes.Length)
                                };

                                byte[] lastErrAckPacketBytes = lastErrAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] lastErrAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, lastErrAckPacketBytes);
                                await App.BrokerSocket.SendAsync(lastErrAckBrokerPacket);
                                PacketBus.Publish(lastErrAckPacketBytes);
                                PacketBus.PublishLog($"Sent GET_LAST_ERR_MSG_ACK successfully");

                                break;

                            case DataPacketTypeEnum.READ_DET_VER_RES:
                                PacketBus.PublishLog("READ_DET_VER_RES received from Client");
                                ReadDetailedVersionAck detailVerAckPayload = new ReadDetailedVersionAck();
                                byte[] detailVerAckPayloadBytes = detailVerAckPayload.Serialize();

                                // Build data packet
                                var detailVerAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.READ_DET_VER_ACK,
                                    PacketPayload = detailVerAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + detailVerAckPayloadBytes.Length)
                                };

                                byte[] detailVerAckPacketBytes = detailVerAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] detailVerAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, detailVerAckPacketBytes);
                                await App.BrokerSocket.SendAsync(detailVerAckBrokerPacket);
                                PacketBus.Publish(detailVerAckPacketBytes);
                                PacketBus.PublishLog($"Sent READ_DET_VER_ACK successfully");

                                break;

                            case DataPacketTypeEnum.CLOSE_RES:
                                PacketBus.PublishLog("CLOSE_RES received from Client");
                                CloseAck closeAckPayload = new CloseAck();
                                byte[] closeAckPayloadBytes = closeAckPayload.Serialize();

                                // Build data packet
                                var closeAckPacket = new DataPacket
                                {
                                    PacketType = DataPacketTypeEnum.CLOSE_ACK,
                                    PacketPayload = closeAckPayloadBytes,
                                    PacketLength = (uint)(4 + 1 + closeAckPayloadBytes.Length)
                                };

                                byte[] closeAckPacketBytes = closeAckPacket.Serialize();

                                // Wrap with Broker protocol
                                byte[] closeAckBrokerPacket =
                                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, closeAckPacketBytes);
                                await App.BrokerSocket.SendAsync(closeAckBrokerPacket);
                                PacketBus.Publish(closeAckPacketBytes);
                                PacketBus.PublishLog($"Sent CLOSE_ACK successfully");

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
