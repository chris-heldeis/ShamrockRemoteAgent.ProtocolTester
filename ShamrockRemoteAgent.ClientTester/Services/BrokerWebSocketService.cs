using ShamrockRemoteAgent.ClientTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Close;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetHardwareStatus;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetLastErrorMsg;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadDetailedVersion;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadVersion;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage;
using System.Net.WebSockets;
using System.Text;

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

                                    // Build LOGIN_ACK payload
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

                                case DataPacketTypeEnum.TX_FRAME_REQ:
                                    PacketBus.PublishLog("TX_FRAME_REQ received from Master");
                                    SendMessageRes txFrameResPayload = new SendMessageRes();
                                    txFrameResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] txFrameResPayloadBytes = txFrameResPayload.Serialize();

                                    // Build data packet
                                    var txFrameResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.TX_FRAME_RES,
                                        PacketPayload = txFrameResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + txFrameResPayloadBytes.Length)
                                    };

                                    byte[] txFrameResPacketBytes = txFrameResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] txFrameResBrokerPacket = 
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, txFrameResPacketBytes);
                                    await App.BrokerSocket.SendAsync(txFrameResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(txFrameResPacketBytes);
                                    PacketBus.PublishLog($"Sent TX_FRAME_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.RX_FRAME_REQ:
                                    PacketBus.PublishLog("RX_FRAME_REQ received from Master");
                                    ReadMessageRes rxFrameResPayload = new ReadMessageRes();
                                    rxFrameResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] apiMsgData = Encoding.UTF8.GetBytes("This is testing api message of the RX_FRAME_RES");
                                    rxFrameResPayload.MsgSize.FieldData = apiMsgData.Length;
                                    rxFrameResPayload.ApiMsg.FieldData = apiMsgData;

                                    byte[] rxFrameResPayloadBytes = rxFrameResPayload.Serialize();

                                    // Build data packet
                                    var rxFrameResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.RX_FRAME_RES,
                                        PacketPayload = rxFrameResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + rxFrameResPayloadBytes.Length)
                                    };

                                    byte[] rxFrameResPacketBytes = rxFrameResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] rxFrameResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, rxFrameResPacketBytes);
                                    await App.BrokerSocket.SendAsync(rxFrameResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(rxFrameResPacketBytes);
                                    PacketBus.PublishLog($"Sent RX_FRAME_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.TX_CMD_REQ:
                                    PacketBus.PublishLog("TX_CMD_REQ received from Master");
                                    SendCommandRes txCmdResPayload = new SendCommandRes();
                                    txCmdResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] txCmdResPayloadBytes = txCmdResPayload.Serialize();

                                    // Build data packet
                                    var txCmdResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.TX_CMD_RES,
                                        PacketPayload = txCmdResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + txCmdResPayloadBytes.Length)
                                    };

                                    byte[] txCmdResPacketBytes = txCmdResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] txCmdResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, txCmdResPacketBytes);
                                    await App.BrokerSocket.SendAsync(txCmdResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(txCmdResPacketBytes);
                                    PacketBus.PublishLog($"Sent TX_CMD_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.READ_VER_REQ:
                                    PacketBus.PublishLog("READ_VER_REQ received from Master");

                                    // Build ReadVersionRes payload
                                    ReadVersionRes readVerResPayload = new ReadVersionRes();

                                    // Convert strings to ASCII bytes
                                    byte[] dmjvBytes = new byte[] { 0xa0};
                                    byte[] dmnvBytes = new byte[] { 0xa0, 0xb0 };
                                    byte[] amjvBytes = new byte[] { 0xa0, 0xb0, 0xc0 };
                                    byte[] amnvBytes = new byte[] { 0x0a, 0x1a, 0x2a, 0x3a };

                                    // Set length fields (1 byte each)
                                    readVerResPayload.DMJVLen.FieldData = dmjvBytes.Length;
                                    readVerResPayload.DMNVLen.FieldData = dmnvBytes.Length;
                                    readVerResPayload.AMJVLen.FieldData = amjvBytes.Length;
                                    readVerResPayload.AMNVLen.FieldData = amnvBytes.Length;

                                    // Set actual string fields
                                    readVerResPayload.DMJV.FieldData = dmjvBytes;
                                    readVerResPayload.DMJV.FieldLength = dmjvBytes.Length;

                                    readVerResPayload.DMNV.FieldData = dmnvBytes;
                                    readVerResPayload.DMNV.FieldLength = dmnvBytes.Length;

                                    readVerResPayload.AMJV.FieldData = amjvBytes;
                                    readVerResPayload.AMJV.FieldLength = amjvBytes.Length;

                                    readVerResPayload.AMNV.FieldData = amnvBytes;
                                    readVerResPayload.AMNV.FieldLength = amnvBytes.Length;

                                    byte[] readVerPayloadBytes = readVerResPayload.Serialize();

                                    var readVerResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.READ_VER_RES,
                                        PacketPayload = readVerPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + readVerPayloadBytes.Length)
                                    };

                                    byte[] readVerResBytes = readVerResPacket.Serialize();

                                    // Show outgoing response
                                    PacketBus.Publish(readVerResBytes);
                                    PacketBus.PublishLog("Sending READ_VER_RES to Master");

                                    // Wrap with Broker protocol
                                    byte[] readVerResBrokerSedingPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, readVerResBytes);

                                    await SendAsync(readVerResBrokerSedingPacket);

                                    break;

                                case DataPacketTypeEnum.GET_ERR_MSG_REQ:
                                    PacketBus.PublishLog("GET_ERR_MSG_REQ received from Master");
                                    GetErrorMsgRes errMsgResPayload = new GetErrorMsgRes();
                                    errMsgResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] errDescData = Encoding.UTF8.GetBytes("This is testing error message description for GET_ERR_MSG_RES");
                                    errMsgResPayload.ErrDescSize.FieldData = errDescData.Length;
                                    errMsgResPayload.ErrDesc.FieldData = errDescData;

                                    byte[] errMsgResPayloadBytes = errMsgResPayload.Serialize();

                                    // Build data packet
                                    var errMsgResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.GET_ERR_MSG_RES,
                                        PacketPayload = errMsgResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + errMsgResPayloadBytes.Length)
                                    };

                                    byte[] errMsgResPacketBytes = errMsgResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] errMsgResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, errMsgResPacketBytes);
                                    await App.BrokerSocket.SendAsync(errMsgResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(errMsgResPacketBytes);
                                    PacketBus.PublishLog($"Sent GET_ERR_MSG_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.GET_HW_STATUS_REQ:
                                    PacketBus.PublishLog("GET_HW_STATUS_REQ received from Master");
                                    GetHardwareStatusRes getHWResPayload = new GetHardwareStatusRes();
                                    getHWResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] hWStatData = new byte[] { 0x18, 0xDA, 0xF1, 0x10 };
                                    getHWResPayload.HwStatusSize.FieldData = hWStatData.Length;
                                    getHWResPayload.HwStatus.FieldData = hWStatData;

                                    byte[] getHWResPayloadBytes = getHWResPayload.Serialize();

                                    // Build data packet
                                    var getHWResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.GET_HW_STATUS_RES,
                                        PacketPayload = getHWResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + getHWResPayloadBytes.Length)
                                    };

                                    byte[] getHWResPacketBytes = getHWResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] getHWResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, getHWResPacketBytes);
                                    await App.BrokerSocket.SendAsync(getHWResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(getHWResPacketBytes);
                                    PacketBus.PublishLog($"Sent GET_HW_STATUS_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.GET_LAST_ERR_MSG_REQ:
                                    PacketBus.PublishLog("GET_LAST_ERR_MSG_REQ received from Master");
                                    GetLastErrorMsgRes lastErrResPayload = new GetLastErrorMsgRes();
                                    lastErrResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    lastErrResPayload.SubErrCode.FieldData = 5000000000;
                                    byte[] lastErrDescData = Encoding.UTF8.GetBytes("This is testing last error message for GET_LAST_ERR_MSG_RES");
                                    lastErrResPayload.ErrDescSize.FieldData = lastErrDescData.Length;
                                    lastErrResPayload.ErrDesc.FieldData = lastErrDescData;

                                    byte[] lastErrResPayloadBytes = lastErrResPayload.Serialize();

                                    // Build data packet
                                    var lastErrResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.GET_LAST_ERR_MSG_RES,
                                        PacketPayload = lastErrResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + lastErrResPayloadBytes.Length)
                                    };

                                    byte[] lastErrResPacketBytes = lastErrResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] lastErrResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, lastErrResPacketBytes);
                                    await App.BrokerSocket.SendAsync(lastErrResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(lastErrResPacketBytes);
                                    PacketBus.PublishLog($"Sent GET_LAST_ERR_MSG_RES successfully!");

                                    break;

                                case DataPacketTypeEnum.READ_DET_VER_REQ:
                                    PacketBus.PublishLog("READ_DET_VER_REQ received from Master");

                                    // Build READ_DET_VER_RES payload
                                    ReadDetailedVersionRes detailVerResPayload = new ReadDetailedVersionRes();

                                    byte[] aviBytes = new byte[] { 0x01 };
                                    byte[] dviBytes = new byte[] { 0x02, 0x03 };
                                    byte[] fviBytes = new byte[] { 0x04, 0x05, 0x06 };

                                    // Set length fields (1 byte each)
                                    detailVerResPayload.AVISize.FieldData = aviBytes.Length;
                                    detailVerResPayload.DVISize.FieldData = dviBytes.Length;
                                    detailVerResPayload.FVISize.FieldData = fviBytes.Length;

                                    // Set actual string fields
                                    detailVerResPayload.AVI.FieldData = aviBytes;
                                    detailVerResPayload.AVI.FieldLength = aviBytes.Length;

                                    detailVerResPayload.DVI.FieldData = dviBytes;
                                    detailVerResPayload.DVI.FieldLength = dviBytes.Length;

                                    detailVerResPayload.FVI.FieldData = fviBytes;
                                    detailVerResPayload.FVI.FieldLength = fviBytes.Length;

                                    byte[] detailVerPayloadBytes = detailVerResPayload.Serialize();

                                    var detailVerResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.READ_DET_VER_RES,
                                        PacketPayload = detailVerPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + detailVerPayloadBytes.Length)
                                    };

                                    byte[] detailVerResBytes = detailVerResPacket.Serialize();

                                    // Show outgoing response
                                    PacketBus.Publish(detailVerResBytes);
                                    PacketBus.PublishLog("Sending READ_DET_VER_RES to Master");

                                    // Wrap with Broker protocol
                                    byte[] detailVerResBrokerSedingPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, detailVerResBytes);

                                    await SendAsync(detailVerResBrokerSedingPacket);

                                    break;

                                case DataPacketTypeEnum.CLOSE_REQ:
                                    PacketBus.PublishLog("CLOSE_REQ received from Master");
                                    CloseRes closeResPayload = new CloseRes();
                                    closeResPayload.ResultCode.FieldData = RPErrorCodeEnum.NO_ERRORS;
                                    byte[] closeResPayloadBytes = closeResPayload.Serialize();

                                    // Build data packet
                                    var closeResPacket = new DataPacket
                                    {
                                        PacketType = DataPacketTypeEnum.CLOSE_RES,
                                        PacketPayload = closeResPayloadBytes,
                                        PacketLength = (uint)(4 + 1 + closeResPayloadBytes.Length)
                                    };

                                    byte[] closeResPacketBytes = closeResPacket.Serialize();
                                    // Wrap with Broker protocol
                                    byte[] closeResBrokerPacket =
                                        BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, closeResPacketBytes);

                                    await App.BrokerSocket.SendAsync(closeResBrokerPacket);
                                    // Publish to HexViewer
                                    PacketBus.Publish(closeResPacketBytes);
                                    PacketBus.PublishLog($"Sent CLOSE_RES successfully!");

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
