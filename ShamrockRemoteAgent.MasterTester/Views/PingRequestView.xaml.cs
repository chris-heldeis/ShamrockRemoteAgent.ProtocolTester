using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views;

public partial class PingRequestView : UserControl
{
    public PingRequestView()
    {
        InitializeComponent();
    }

    private async void OnSendPingClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            // Build empty payload
            var payload = new PingReq();
            byte[] payloadBytes = payload.Serialize(); // should be empty array

            // Build data packet
            var packet = new DataPacket
            {
                PacketType = DataPacketTypeEnum.PING_REQ,
                PacketPayload = payloadBytes,
                PacketLength = (uint)(4 + 1 + payloadBytes.Length)
            };

            byte[] packetBytes = packet.Serialize();

            // Wrap with Broker protocol
            byte[] brokerPacket =
                BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, packetBytes);

            await App.BrokerSocket.SendAsync(brokerPacket);

            // Publish to HexViewer
            PacketBus.Publish(packetBytes);
            PacketBus.PublishLog($"Sent PingReq successfully!");
        }
        catch (Exception ex)
        {
            PacketBus.PublishLog($"ERROR: {ex.Message}");
        }
    }
}
