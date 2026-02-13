using ShamrockRemoteAgent.ClientTester.Helpers;
using ShamrockRemoteAgent.ClientTester.Models;
using ShamrockRemoteAgent.ClientTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.ClientTester.Views
{
    /// <summary>
    /// Interaction logic for HexViewerView.xaml
    /// </summary>
    public partial class HexViewerView : UserControl
    {
        public HexViewerView()
        {
            InitializeComponent();

            PacketBus.PacketBuilt += OnPacketBuilt;
            PacketBus.PacketDecoded += OnPacketDecoded;
            PacketBus.BrokerPacketDecoded += OnBrokerPacketDecoded;
            PacketBus.LogPublished += OnLog;
        }

        private void OnPacketBuilt(byte[] data)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLine("");
                AppendLine("[PACKET]");
                AppendLine(HexFormatter.ToHex(data));
            });
        }
        private void OnPacketDecoded(DecodedPacket packet)
        {
            Dispatcher.Invoke(() =>
            {
                DetailsBox.Text =
$"""
Packet Type: {((DataPacketTypeEnum)packet.PacketType).ToString()}
Packet Length: {packet.PacketLength}
{packet.PayloadDetails}
""";
            });
        }

        private void OnBrokerPacketDecoded(BrokerDecodedPacket packet)
        {
            Dispatcher.Invoke(() =>
            {
                DetailsBox.Text =
$"""
Packet Length: {packet.TotalLength}
Packet Type: {((BrokerPacketType)packet.PacketType).ToString()}
Packet Seq: {packet.Sequence}
Packet Payload: {packet.Payload}
""";
            });
        }

        private void OnLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLine(
                    $"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }

        private void AppendLine(string text)
        {
            HexBox.AppendText(text + Environment.NewLine);
            HexBox.ScrollToEnd();
        }
    }
}
