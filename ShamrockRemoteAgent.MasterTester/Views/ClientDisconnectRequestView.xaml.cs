using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester.Views
{
    public partial class ClientDisconnectRequestView : UserControl
    {
        public ClientDisconnectRequestView()
        {
            InitializeComponent();
        }

        private async void OnSendDisconnectClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(ClientIdTextBox.Text, out ushort clientId))
                {
                    PacketBus.PublishLog("Invalid CLIENT_ID");
                    return;
                }

                if (!App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.ConnectAsync(
                        App.BrokerHost,
                        App.BrokerPort,
                        App.MasterId);
                }

                // CLIENT_ID → 2 bytes BE
                byte[] payload =
                {
                    (byte)(clientId >> 8),
                    (byte)(clientId & 0xFF)
                };

                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.CLI_DISCON_REQ,
                    PacketPayload = payload,
                    PacketLength = (uint)(4 + 1 + payload.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Show hex in HexViewer
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog($"Sent CLI_DISCON_REQ for ClientId={clientId}");

                await App.BrokerSocket.SendAsync(packetBytes);
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }
    }
}
