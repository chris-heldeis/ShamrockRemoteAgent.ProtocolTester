using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadDetailedVersion;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class ReadDetailedVersionRequestView : UserControl
    {
        public ReadDetailedVersionRequestView()
        {
            InitializeComponent();
        }

        private async void OnReadDetailedVersionClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId) || clientId == 0)
                {
                    MessageBox.Show("Client ID must be greater than 0.");
                    ClientIdBox.Focus();
                    return;
                }

                App.CheckConnect();

                // Build payload
                var payload = new ReadDetailedVersionReq
                {
                    ClientID = { FieldData = clientId }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.READ_DET_VER_REQ,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Wrap with Broker protocol
                byte[] brokerPacket =
                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, packetBytes);

                // Publish to HexViewer
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog($"Sent READ_DET_VER_REQ successfully!");

                // Send
                await App.BrokerSocket.SendAsync(brokerPacket);
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }
    }
}
