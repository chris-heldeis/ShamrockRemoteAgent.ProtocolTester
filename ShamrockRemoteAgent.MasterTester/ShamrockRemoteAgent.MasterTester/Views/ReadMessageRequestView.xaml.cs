using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Client;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.MasterTester.Services;
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

            // Default selection for BlockOnRead
            BlockOnReadBox.SelectedIndex = 0;
        }

        private async void OnReadMessageClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId))
                {
                    MessageBox.Show("Invalid Client ID");
                    return;
                }

                if (!ushort.TryParse(BufferSizeBox.Text, out ushort bufferSize))
                {
                    MessageBox.Show("Invalid Buffer Size");
                    return;
                }

                byte blockValue = (byte)(BlockOnReadBox.SelectedIndex == 0 ? 0x01 : 0x00); // 0x01 = BLOCKING, 0x00 = NON_BLOCKING

                // Build payload
                var payload = new ClientReadMessageRequest
                {
                    ClientId = clientId,
                    BufferSize = bufferSize,
                    BlockOnRead = blockValue
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

                // Send to broker
                if (App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.SendAsync(packetBytes);
                    PacketBus.PublishLog($"ReadMessageRequest sent to Client {clientId}");
                }
                else
                {
                    MessageBox.Show("Broker WebSocket not connected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
