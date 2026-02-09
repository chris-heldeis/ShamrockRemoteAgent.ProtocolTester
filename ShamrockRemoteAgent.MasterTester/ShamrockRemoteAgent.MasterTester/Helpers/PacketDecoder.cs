using ShamrockRemoteAgent.MasterTester.Models;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Models.DataPackets;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login;
using ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShamrockRemoteAgent.MasterTester.Helpers
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

                DataPacketTypeEnum.LOGIN_REQ =>
                    LoginReq.Deserialize(packet.PacketPayload),

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

            var fieldsProp = payload.GetType().GetProperty("Fields");
            if (fieldsProp?.GetValue(payload) is Array fields)
            {
                foreach (var field in fields)
                {
                    var type = field.GetType();
                    var name = type.GetProperty("FieldType")?.GetValue(field);
                    var value = type.GetProperty("FieldData")?.GetValue(field);
                    sb.AppendLine($"- {name}: {value}");
                }
            }

            return sb.ToString();
        }
    }
}
