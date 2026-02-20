using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.MasterTester.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage;
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
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId) || clientId == 0)
                {
                    MessageBox.Show("Client ID must be greater than 0.");
                    return;
                }

                // Payload Box
                string payloadText = ClientMsgBox.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(payloadText))
                {
                    MessageBox.Show("Message payload cannot be empty.");
                    ClientMsgBox.Focus();
                    return;
                }

                byte[] clientMsg = MessageConverter.StringToBytes(ClientMsgBox.Text ?? "");

                ushort msgSize = (ushort)clientMsg.Length;

                // Build payload
                SendMessageReq payload = new SendMessageReq
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
                PacketBus.PublishLog($"Sent TX_FRAME_REQ successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
