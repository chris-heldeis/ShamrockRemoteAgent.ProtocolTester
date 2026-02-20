using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class GetErrorMsgRequestView : UserControl
    {
        public GetErrorMsgRequestView()
        {
            InitializeComponent();
        }

        private async void OnGetErrorMsgClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ErrorCodeBox.Text, out ushort errorCode))
                {
                    MessageBox.Show("Invalid Error Code");
                    ErrorCodeBox.Focus();
                    return;
                }

                // Build payload
                var payload = new GetErrorMsgReq
                {
                    ErrCode = { FieldData = errorCode }
                };

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.GET_ERR_MSG_REQ,
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
                PacketBus.PublishLog($"Sent GET_ERR_MSG_REQ successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
