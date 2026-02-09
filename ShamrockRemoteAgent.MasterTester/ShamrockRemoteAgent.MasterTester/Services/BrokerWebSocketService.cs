using ShamrockRemoteAgent.MasterTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
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
                    var decoded = PacketDecoder.Decode(brokerRecivingPacket.Payload);
                    PacketBus.Publish(brokerRecivingPacket.Payload);

                    PacketBus.PublishLog(
                        $"Received {brokerRecivingPacket.Payload.Length} bytes");
                    PacketBus.PublishDecoded(decoded);

                    // Sent Response
                    if (decoded.PacketType == DataPacketTypeEnum.PING_RES)
                    {
                        PacketBus.PublishLog("PING_RES received from Client");
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
