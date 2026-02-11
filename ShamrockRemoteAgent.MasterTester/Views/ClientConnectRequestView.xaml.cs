using System.Windows;
using System.Windows.Controls;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ClientConnectReq;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.MasterTester.Services;

namespace ShamrockRemoteAgent.MasterTester.Views;

public partial class ClientConnectRequestView : UserControl
{
    public ClientConnectRequestView()
    {
        InitializeComponent();

        ProtocolBox.ItemsSource = Enum.GetValues(typeof(DeviceProtocolEnum));
        ProtocolBox.SelectedIndex = 1;

        SelfPacketizeBox.ItemsSource = Enum.GetValues(typeof(SelfPacketizeEnum));
        SelfPacketizeBox.SelectedIndex = 0;
    }

    private async void OnClientConnectRequestClicked(object sender, RoutedEventArgs e)
    {
        ushort deviceId = ushort.Parse(DeviceIdBox.Text);
        uint txBuf = uint.Parse(TxBufBox.Text);
        uint rxBuf = uint.Parse(RxBufBox.Text);

        var payload = new ClientConnectReq
        {
            DeviceID = { FieldData = deviceId },
            Protocol = { FieldData = (DeviceProtocolEnum)ProtocolBox.SelectedItem! },
            TxBufSize = { FieldData = txBuf },
            RxBufSize = { FieldData = rxBuf },
            SelfPacketize = { FieldData = (SelfPacketizeEnum)SelfPacketizeBox.SelectedItem! }
        };

        byte[] payloadBytes = payload.Serialize();

        var packet = new DataPacket
        {
            PacketType = DataPacketTypeEnum.CLI_CON_REQ,
            PacketPayload = payloadBytes,
            PacketLength = (uint)(4 + 1 + payloadBytes.Length)
        };

        byte[] packetBytes = packet.Serialize();

        // Wrap with Broker protocol
        byte[] brokerPacket =
            BrokerProtocol.Encode(PacketType.COM_DATA, packetBytes);

        await App.BrokerSocket.SendAsync(brokerPacket);

        PacketBus.Publish(packetBytes);
        PacketBus.PublishLog($"Sent ClientConnectRequest successfully!");
    }
}
