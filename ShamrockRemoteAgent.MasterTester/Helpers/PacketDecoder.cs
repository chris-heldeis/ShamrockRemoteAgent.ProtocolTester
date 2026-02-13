using ShamrockRemoteAgent.MasterTester.Models;
using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
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
