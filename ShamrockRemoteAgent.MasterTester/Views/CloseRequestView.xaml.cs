using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Close;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class CloseRequestView : UserControl
    {
        public CloseRequestView()
        {
            InitializeComponent();
        }

        private async void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.ConnectAsync(
                        App.BrokerHost,
                        App.BrokerPort,
                        App.MasterId);
                }

                // Build payload
                var payload = new CloseReq();

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.CLOSE_REQ,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Wrap with Broker protocol
                byte[] brokerPacket =
                    BrokerProtocol.Encode(PacketType.COM_DATA, packetBytes);

                // Show in HexViewer
                PacketBus.Publish(brokerPacket);
                PacketBus.PublishLog("Sent CLOSE_REQ");

                await App.BrokerSocket.SendAsync(brokerPacket);
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }
    }
}
