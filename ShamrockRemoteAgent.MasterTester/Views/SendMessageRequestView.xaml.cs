using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class SendMessageRequestView : UserControl
    {
        public SendMessageRequestView()
        {
            InitializeComponent();
        }

        private async void OnSendMessageClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId))
                {
                    MessageBox.Show("Invalid Client ID");
                    return;
                }

                byte[] clientMsg = ParseMessage(ClientMsgBox.Text);

                ushort msgSize = (ushort)clientMsg.Length;

                // Build payload
                var payload = new SendMessageReq
                {
                    ClientID = { FieldData = clientId },
                    MsgSize = { FieldData = msgSize },
                    ClientMsg = { FieldData = clientMsg },
                    StatusOnTx = { FieldData = 0x00 },
                    BlockOnSend = { FieldData = 0x00 }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.TX_FRAME_REQ,
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
                PacketBus.PublishLog($"Sent SendMessageRequest successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private byte[] ParseMessage(string input)
        {
            input = input.Trim();

            // Hex input (space separated)
            if (input.Contains(" "))
            {
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] bytes = new byte[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                    bytes[i] = Convert.ToByte(parts[i], 16);

                return bytes;
            }

            return Encoding.ASCII.GetBytes(input);
        }
    }
}
