using System;
using System.IO;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Client
{
    public class ClientReadMessageRequest
    {
        public ushort ClientId { get; set; }      // 2 bytes
        public ushort BufferSize { get; set; }    // 2 bytes
        public byte BlockOnRead { get; set; }     // 1 byte

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(ClientId);       // little-endian 2 bytes
            writer.Write(BufferSize);     // little-endian 2 bytes
            writer.Write(BlockOnRead);    // 1 byte

            return ms.ToArray();
        }

        public static ClientReadMessageRequest Deserialize(byte[] data)
        {
            if (data.Length < 5)
                throw new ArgumentException("Invalid data length for ClientReadMessageRequest");

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            return new ClientReadMessageRequest
            {
                ClientId = reader.ReadUInt16(),
                BufferSize = reader.ReadUInt16(),
                BlockOnRead = reader.ReadByte()
            };
        }
    }
}
