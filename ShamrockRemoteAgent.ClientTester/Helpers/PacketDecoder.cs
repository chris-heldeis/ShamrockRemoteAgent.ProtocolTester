using ShamrockRemoteAgent.ClientTester.Models;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage;

namespace ShamrockRemoteAgent.ClientTester.Helpers
{
    public static class PacketDecoder
    {
        public static DecodedPacket Decode(byte[] rawBytes)
        {
            var packet = DataPacket.Deserialize(rawBytes);
            if (packet == null)
                throw new Exception("Invalid DataPacket");

            object? payload = packet.PacketType switch
            {
                DataPacketTypeEnum.PING_REQ =>
                    PingReq.Deserialize(),

                DataPacketTypeEnum.LOGIN_RES =>
                    LoginRes.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.CLI_CON_REQ =>
                    ClientConnectReq.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.CLI_CON_ACK =>
                    ClientConnectAck.Deserialize(),

                DataPacketTypeEnum.CLI_DISCON_REQ =>
                    ClientDisconnectReq.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.CLI_DISCON_ACK =>
                    ClientDisconnectAck.Deserialize(),

                DataPacketTypeEnum.TX_FRAME_REQ =>
                    SendMessageReq.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.TX_FRAME_ACK =>
                    SendMessageAck.Deserialize(),

                DataPacketTypeEnum.RX_FRAME_REQ =>
                    ReadMessageReq.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.RX_FRAME_ACK =>
                    ReadMessageAck.Deserialize(),

                DataPacketTypeEnum.TX_CMD_REQ =>
                    SendCommandReq.Deserialize(packet.PacketPayload),

                DataPacketTypeEnum.TX_CMD_ACK =>
                    SendCommandAck.Deserialize(),

                _ => null
            };

            return new DecodedPacket
            {
                RawBytes = rawBytes,
                PacketType = packet.PacketType,
                PacketLength = packet.PacketLength,
                Payload = payload,
                PayloadDetails = BuildPayloadDetails(payload)
            };
        }

        private static string BuildPayloadDetails(object? payload)
        {
            if (payload == null)
                return "(no payload)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Payload Fields:");

            var properties = payload.GetType().GetProperties();

            foreach (var prop in properties)
            {
                // We only care about properties of type IField
                if (!typeof(IField).IsAssignableFrom(prop.PropertyType))
                    continue;

                var field = prop.GetValue(payload);
                if (field == null)
                    continue;

                var fieldDataProp = field.GetType().GetProperty("FieldData");
                var value = fieldDataProp?.GetValue(field);

                sb.AppendLine($"- {prop.Name}: {value}");
            }

            return sb.ToString();
        }
    }
}
