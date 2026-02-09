using ShamrockRemoteAgent.ClientTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShamrockRemoteAgent.ClientTester.ViewModels
{
    public class LoginRequestViewModel : INotifyPropertyChanged
    {
        public string CustomerID { get; set; } = "";
        public string Email { get; set; } = "";
        public ushort DeviceID { get; set; }

        public DeviceProtocolEnum Protocol { get; set; }
        public DeviceAdapterEnum Adapter { get; set; }
        public ServiceTypeEnum ServiceType { get; set; }

        public string OtherService { get; set; } = "";

        private string _hexOutput = "";
        public string HexOutput
        {
            get => _hexOutput;
            set { _hexOutput = value; OnPropertyChanged(); }
        }

        public Array ProtocolValues => Enum.GetValues(typeof(DeviceProtocolEnum));
        public Array AdapterValues => Enum.GetValues(typeof(DeviceAdapterEnum));
        public Array ServiceTypeValues => Enum.GetValues(typeof(ServiceTypeEnum));

        public async void BuildPacket()
        {
            try
            {
                if (!App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.ConnectAsync(
                        App.BrokerHost,
                        App.BrokerPort,
                        App.ClientId);
                }

                LoginReq payload = new LoginReq();

                payload.CustomerID.FieldData = CustomerID;
                payload.Email.FieldData = Email;
                payload.DeviceID.FieldData = DeviceID;
                payload.Protocol.FieldData = Protocol;
                payload.Adapter.FieldData = Adapter;
                payload.ServiceType.FieldData = ServiceType;
                payload.OtherService.FieldData = OtherService;

                byte[] payloadBytes = payload.Serialize();

                // Build data packet
                var packet = new DataPacket
                {
                    PacketType = DataPacketTypeEnum.LOGIN_REQ,
                    PacketPayload = payloadBytes,
                    PacketLength = (uint)(4 + 1 + payloadBytes.Length)
                };

                byte[] packetBytes = packet.Serialize();

                // Publish to HexViewer
                PacketBus.Publish(packetBytes);

                await App.BrokerSocket.SendAsync(packetBytes);
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"ERROR: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
