using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ClientConnectReq;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShamrockRemoteAgent.MasterTester.ViewModels
{
    public class ClientConnectRequestViewModel : INotifyPropertyChanged
    {
        public ushort DeviceID { get; set; }

        public DeviceProtocolEnum Protocol { get; set; }

        public uint TxBufSize { get; set; }

        public uint RxBufSize { get; set; }

        public SelfPacketizeEnum SelfPacketize { get; set; }

        public Array ProtocolValues =>
            Enum.GetValues(typeof(DeviceProtocolEnum));

        public Array SelfPacketizeValues =>
            Enum.GetValues(typeof(SelfPacketizeEnum));

        public bool Validate(out string error)
        {
            error = string.Empty;

            if (DeviceID == 0)
            {
                error = "Device ID must be greater than 0.";
                return false;
            }

            if ((int)Protocol == 0)
            {
                error = "Protocol must be selected.";
                return false;
            }

            return true;
        }

        public async void SendClientConnectRequest()
        {
            try
            {
                ClientConnectReq payload = new ClientConnectReq();

                payload.DeviceID.FieldData = DeviceID;
                payload.Protocol.FieldData = Protocol;
                payload.TxBufSize.FieldData = TxBufSize;
                payload.RxBufSize.FieldData = RxBufSize;
                payload.SelfPacketize.FieldData = SelfPacketize;

                byte[] payloadBytes = payload.Serialize();

                // Build DataPacket
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.CLI_CON_ACK,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Wrap with Broker protocol
                byte[] brokerPacket =
                    BrokerProtocol.Encode(
                        BrokerPacketTypeEnum.COM_DATA,
                        packetBytes);

                await App.BrokerSocket.SendAsync(brokerPacket);

                // Publish to HexViewer (same as Login)
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog("Sent ClientConnectReq successfully");
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
        }
    }
}
