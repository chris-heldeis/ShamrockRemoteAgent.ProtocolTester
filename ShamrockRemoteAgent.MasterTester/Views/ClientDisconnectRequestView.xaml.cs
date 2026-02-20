using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class ClientDisconnectRequestView : UserControl
    {
        public ClientDisconnectRequestView()
        {
            InitializeComponent();
        }

        private async void OnSendDisconnectClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdTextBox.Text, out ushort clientId) || clientId == 0)
                {
                    PacketBus.PublishLog("Client ID must be greater than 0.");
                    ClientIdTextBox.Focus();
                    return;
                }

                App.CheckConnect();

                // Build payload using protocol model
                var payload = new ClientDisconnectReq
                {
                    ClientID = { FieldData = clientId }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.CLI_DISCON_REQ,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Wrap with Broker protocol
                byte[] brokerPacket =
                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, packetBytes);

                // Publish to HexViewer
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog($"Sent CLI_DISCON_REQ successfully");

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
