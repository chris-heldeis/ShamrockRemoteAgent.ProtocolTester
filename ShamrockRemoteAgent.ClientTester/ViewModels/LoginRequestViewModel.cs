using ShamrockRemoteAgent.ClientTester.Services;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

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
        public Array ProtocolValues => Enum.GetValues(typeof(DeviceProtocolEnum));
        public Array AdapterValues => Enum.GetValues(typeof(DeviceAdapterEnum));
        public Array ServiceTypeValues => Enum.GetValues(typeof(ServiceTypeEnum));

        private string _hexOutput = "";
        public string HexOutput
        {
            get => _hexOutput;
            set { _hexOutput = value; OnPropertyChanged(); }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        public bool Validate(out string error)
        {
            error = string.Empty;

            // CustomerID validation (max 32 BYTES, not chars)
            if (string.IsNullOrWhiteSpace(CustomerID))
            {
                error = "Customer ID is required.";
                return false;
            }

            if (Encoding.UTF8.GetByteCount(CustomerID) > 32)
            {
                error = "Customer ID must not exceed 32 bytes.";
                return false;
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(Email))
            {
                error = "Email is required.";
                return false;
            }

            if (Encoding.UTF8.GetByteCount(Email) > 256)
            {
                error = "Email must not exceed 256 bytes.";
                return false;
            }

            if (!IsValidEmail(Email))
            {
                error = "Invalid email format.";
                return false;
            }

            // DeviceID validation
            if (DeviceID == 0)
            {
                error = "Device ID must be a valid positive number.";
                return false;
            }

            // Protocol validation
            if ((int)Protocol == 0)
            {
                error = "Protocol must be selected.";
                return false;
            }

            // Adapter validation
            if ((int)Adapter == 0)
            {
                error = "Adapter must be selected.";
                return false;
            }

            // ServiceType validation
            if ((int)ServiceType == 0)
            {
                error = "Service Type must be selected.";
                return false;
            }

            return true;
        }
        public async void SendLoginRequest()
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
                // Wrap with Broker protocol
                byte[] brokerPacket =
                    BrokerProtocol.Encode(BrokerPacketTypeEnum.COM_DATA, packetBytes);

                await App.BrokerSocket.SendAsync(brokerPacket);
                // Publish to HexViewer
                PacketBus.Publish(packetBytes);
                PacketBus.PublishLog($"Sent LoginReq successfully!");
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
