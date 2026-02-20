using ShamrockRemoteAgent.MasterTester.Helpers;
using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class SendCommandRequestView : UserControl
    {
        public SendCommandRequestView()
        {
            InitializeComponent();
        }

        private async void OnSendCommandClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // CMD_NUM
                if (!ushort.TryParse(CommandNumBox.Text, out ushort cmdNum))
                {
                    MessageBox.Show("Invalid Command Number");
                    CommandNumBox.Focus();
                    return;
                }

                // CLIENT_ID
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId) || clientId == 0)
                {
                    MessageBox.Show("Client ID must be greater than 0.");
                    ClientIdBox.Focus();
                    return;
                }

                // Payload Box
                string payloadText = CommandPayloadBox.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(payloadText))
                {
                    MessageBox.Show("Command payload cannot be empty.");
                    CommandPayloadBox.Focus();
                    return;
                }

                // CLIENT_CMD (hex or text)
                byte[] commandPayload = MessageConverter.StringToBytes(CommandPayloadBox.Text ?? string.Empty);

                ushort msgSize = (ushort)commandPayload.Length;

                // Build payload
                var payload = new SendCommandReq
                {
                    CmdNum = { FieldData = cmdNum },
                    ClientID = { FieldData = clientId },
                    MsgSize = { FieldData = msgSize },
                    ClientCmd = { FieldData = commandPayload }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.TX_CMD_REQ,
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
                PacketBus.PublishLog($"Sent SendCommandRequest successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
