using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using System.IO;
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
                if (!App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.ConnectAsync(
                        App.BrokerHost,
                        App.BrokerPort,
                        App.MasterId);
                }

                // --- Parse inputs ---
                ushort clientId = ushort.Parse(ClientIdBox.Text.Trim());
                byte statusOnTx = 0;
                byte blockOnSend = 0;

                byte[] messageBytes = Encoding.UTF8.GetBytes(
                    ClientMsgBox.Text ?? string.Empty);

                ushort msgSize;
                if (!ushort.TryParse(MsgSizeBox.Text, out msgSize) || msgSize == 0)
                {
                    msgSize = (ushort)messageBytes.Length;
                    MsgSizeBox.Text = msgSize.ToString();
                }

                // --- Build payload manually (protocol order) ---
                using var ms = new MemoryStream();

                ms.Write(BitConverter.GetBytes(clientId).Reverse().ToArray()); // 2 bytes
                ms.Write(BitConverter.GetBytes(msgSize).Reverse().ToArray());  // 2 bytes
                ms.Write(messageBytes);                                        // variable
                ms.WriteByte(statusOnTx);                                      // 1 byte
                ms.WriteByte(blockOnSend);                                     // 1 byte

                byte[] payloadBytes = ms.ToArray();

                // --- Build data packet ---
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.TX_FRAME_REQ,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // --- HexViewer ---
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog(
                    $"TX_FRAME_REQ → Client {clientId}, {msgSize} bytes");

                // --- Send ---
                await App.BrokerSocket.SendAsync(packetBytes);
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }
    }
}
