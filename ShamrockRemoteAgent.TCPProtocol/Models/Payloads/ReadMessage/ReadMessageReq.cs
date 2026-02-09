using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ReadMessageReq;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage
{
    public class ReadMessageReq: Payload
    {
        private static int ClientIDLen = 2;
        private static int BufferSizeLen = 2;
        private static int BlockOnReadLen = 1;

        public IField ClientID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ClientIDLen,
            FieldData = 0,
        };
        public IField BufferSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = BufferSizeLen,
            FieldData = 0,
        };

        public IField BlockOnRead { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = BlockOnReadLen,
            FieldData = BlockOnReadTypeEnum.NOT_INITIALIZED,
        };

        public new IField[] Fields
        {
            get
            {
                return [
                    ClientID,
                    BufferSize,
                    BlockOnRead,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    ClientIDLen,
                    BufferSizeLen,
                    BlockOnReadLen,
                ]))
                return null;

            ReadMessageReq payload = new ReadMessageReq();

            int curPos = 0;
            byte[] clientIDData = bytes[curPos..(curPos + ClientIDLen)];
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(clientIDData);
            curPos += ClientIDLen;

            byte[] bufferSizeData = bytes[curPos..(curPos + BufferSizeLen)];
            payload.BufferSize.FieldData = TypeConverter.BytesToNumber<ushort>(bufferSizeData);
            curPos += BufferSizeLen;

            byte[] blockOnReadData = bytes[curPos..(curPos + BlockOnReadLen)];
            payload.BlockOnRead.FieldData = TypeConverter.BytesToNumber<BlockOnReadTypeEnum>(blockOnReadData);
            curPos += BlockOnReadLen;

            return payload;
        }
    }
}
