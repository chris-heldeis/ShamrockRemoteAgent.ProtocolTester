using ShamrockRemoteAgent.ClientTester.Helpers;
using ShamrockRemoteAgent.ClientTester.Models;
using ShamrockRemoteAgent.ClientTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using System.Windows;
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
            ReplayHistory();
            this.Unloaded += HexViewerView_Unloaded;
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
                DetailsBox.Text +=
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
                DetailsBox.Text +=
$"""

Packet Length: {packet.TotalLength}
Packet Type: {((BrokerPacketTypeEnum)packet.PacketType).ToString()}
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

        private void ReplayHistory()
        {
            // Replay packets
            foreach (var packet in PacketBus.PacketHistory)
                OnPacketBuilt(packet);

            // Replay decoded packets
            foreach (var decoded in PacketBus.DecodedHistory)
                OnPacketDecoded(decoded);

            // Replay broker decoded packets
            foreach (var broker in PacketBus.BrokerDecodedHistory)
                OnBrokerPacketDecoded(broker);

            // Replay logs
            foreach (var log in PacketBus.LogHistory)
                OnLog(log);
        }

        private void HexViewerView_Unloaded(object sender, RoutedEventArgs e)
        {
            PacketBus.PacketBuilt -= OnPacketBuilt;
            PacketBus.PacketDecoded -= OnPacketDecoded;
            PacketBus.BrokerPacketDecoded -= OnBrokerPacketDecoded;
            PacketBus.LogPublished -= OnLog;

            this.Unloaded -= HexViewerView_Unloaded; // optional cleanup
        }
    }
}
