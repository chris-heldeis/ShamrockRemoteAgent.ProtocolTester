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
                // CLIENT_ID
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId))
                {
                    MessageBox.Show("Invalid Client ID");
                    return;
                }

                // CMD_NUM
                if (!ushort.TryParse(CommandNumBox.Text, out ushort cmdNum))
                {
                    MessageBox.Show("Invalid Command Number");
                    return;
                }

                // CLIENT_CMD (hex or text)
                byte[] commandPayload;
                string rawText = CommandPayloadBox.Text.Trim();

                if (string.IsNullOrEmpty(rawText))
                {
                    commandPayload = Array.Empty<byte>();
                }
                else if (rawText.Contains(" "))
                {
                    // HEX input: "01 02 FF"
                    commandPayload = rawText
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => byte.Parse(b, NumberStyles.HexNumber))
                        .ToArray();
                }
                else
                {
                    // Plain text
                    commandPayload = System.Text.Encoding.ASCII.GetBytes(rawText);
                }

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
