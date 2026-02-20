using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetLastErrorMsg;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class GetLastErrorMsgRequestView : UserControl
    {
        public GetLastErrorMsgRequestView()
        {
            InitializeComponent();
        }

        private async void OnGetLastErrorMsgClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdBox.Text, out ushort clientId) || clientId == 0)
                {
                    MessageBox.Show("Client ID must be greater than 0.");
                    ClientIdBox.Focus();
                    return;
                }

                if (!ushort.TryParse(ErrorCodeBox.Text, out ushort errorCode))
                {
                    MessageBox.Show("Invalid Error Code");
                    ErrorCodeBox.Focus();
                    return;
                }

                // Build payload
                var payload = new GetLastErrorMsgReq
                {
                    ClientID = { FieldData = clientId },
                    ErrCode = { FieldData = errorCode }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.GET_LAST_ERR_MSG_REQ,
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
                PacketBus.PublishLog($"Sent GET_LAST_ERR_MSG_REQ successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
