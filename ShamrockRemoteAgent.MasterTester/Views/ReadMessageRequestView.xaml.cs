using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ReadMessageReq;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class ReadMessageRequestView : UserControl
    {
        public ReadMessageRequestView()
        {
            InitializeComponent();

            BlockOnReadBox.ItemsSource =
                Enum.GetValues(typeof(BlockOnReadTypeEnum))
                    .Cast<BlockOnReadTypeEnum>();
        }

        private async void OnReadMessageClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId) || clientId == 0)
                {
                    MessageBox.Show("Invalid Client ID");
                    return;
                }

                if (!ushort.TryParse(BufferSizeBox.Text, out ushort bufferSize) || bufferSize == 0)
                {
                    MessageBox.Show("Invalid Buffer Size");
                    return;
                }

                if (BlockOnReadBox.SelectedItem is not BlockOnReadTypeEnum blockType || blockType == 0)
                {
                    MessageBox.Show("Select BlockOnRead Type");
                    return;
                }

                // Build payload
                var payload = new ReadMessageReq
                {
                    ClientID = { FieldData = clientId },
                    BufferSize = { FieldData = bufferSize },
                    BlockOnRead = { FieldData = blockType },
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.RX_FRAME_REQ,
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
                PacketBus.PublishLog($"Sent RX_FRAME_REQ successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
