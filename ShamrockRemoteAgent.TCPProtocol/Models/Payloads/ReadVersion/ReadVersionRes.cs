using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadVersion
{
    public class ReadVersionRes: Payload
    {
        private static int DMJVLenLen = 1;
        private static int DMNVLenLen = 1;
        private static int AMJVLenLen = 1;
        private static int AMNVLenLen = 1;
        public IField DMJVLen { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = DMJVLenLen,
            FieldData = 0,
        };
        public IField DMNVLen { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = DMNVLenLen,
            FieldData = 0,
        };
        public IField AMJVLen { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = AMJVLenLen,
            FieldData = 0,
        };
        public IField AMNVLen { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = AMNVLenLen,
            FieldData = 0,
        };
        public IField DMJV { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField DMNV { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField AMJV { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField AMNV { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    DMJVLen,
                    DMNVLen,
                    AMJVLen,
                    AMNVLen,
                    DMJV,
                    DMNV,
                    AMJV,
                    AMNV,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            ReadVersionRes payload = new ReadVersionRes();

            int curPos = 0;
            byte[] DMJVLenData = bytes[curPos..(curPos + DMJVLenLen)];
            int DMJVLenDataLen = TypeConverter.BytesToNumber<ushort>(DMJVLenData);
            payload.DMJVLen.FieldData = DMJVLenDataLen;
            curPos += DMJVLenLen;

            byte[] DMNVLenData = bytes[curPos..(curPos + DMNVLenLen)];
            int DMNVLenDataLen = TypeConverter.BytesToNumber<ushort>(DMNVLenData);
            payload.DMNVLen.FieldData = DMNVLenDataLen;
            curPos += DMNVLenLen;

            byte[] AMJVLenData = bytes[curPos..(curPos + AMJVLenLen)];
            int AMJVLenDataLen = TypeConverter.BytesToNumber<ushort>(AMJVLenData);
            payload.AMJVLen.FieldData = AMJVLenDataLen;
            curPos += AMJVLenLen;

            byte[] AMNVLenData = bytes[curPos..(curPos + AMNVLenLen)];
            int AMNVLenDataLen = TypeConverter.BytesToNumber<ushort>(AMNVLenData);
            payload.AMNVLen.FieldData = AMNVLenDataLen;
            curPos += AMNVLenLen;

            byte[] DMJVData = bytes[curPos..(curPos + DMJVLenDataLen)];
            payload.DMJV.FieldData = TypeConverter.BytesToNumber<ushort>(DMJVData);
            payload.DMJV.FieldLength = DMJVLenDataLen;
            curPos += DMJVLenDataLen;

            byte[] DMNVData = bytes[curPos..(curPos + DMNVLenDataLen)];
            payload.DMNV.FieldData = TypeConverter.BytesToNumber<ushort>(DMNVData);
            payload.DMNV.FieldLength = DMNVLenDataLen;
            curPos += DMNVLenDataLen;

            byte[] AMJVData = bytes[curPos..(curPos + AMJVLenDataLen)];
            payload.AMJV.FieldData = TypeConverter.BytesToNumber<ushort>(AMJVData);
            payload.AMJV.FieldLength = AMJVLenDataLen;
            curPos += AMJVLenDataLen;

            byte[] AMNVData = bytes[curPos..(curPos + AMNVLenDataLen)];
            payload.AMNV.FieldData = TypeConverter.BytesToNumber<ushort>(AMNVData);
            payload.AMNV.FieldLength = AMNVLenDataLen;
            curPos += AMNVLenDataLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = DMJVLenLen + DMNVLenLen + AMJVLenLen + AMNVLenLen;
            byte[] DMJVData = bytes[curPos..(curPos + DMJVLenLen)];
            int DMJVDataLen = TypeConverter.BytesToNumber<ushort>(DMJVData);
            curPos += DMJVLenLen;

            byte[] DMNVData = bytes[curPos..(curPos + DMNVLenLen)];
            int DMNVDataLen = TypeConverter.BytesToNumber<ushort>(DMNVData);
            curPos += DMNVLenLen;

            byte[] AMJVData = bytes[curPos..(curPos + AMJVLenLen)];
            int AMJVDataLen = TypeConverter.BytesToNumber<ushort>(AMJVData);
            curPos += AMJVLenLen;

            byte[] AMNVData = bytes[curPos..(curPos + AMNVLenLen)];
            int AMNVDataLen = TypeConverter.BytesToNumber<ushort>(AMNVData);
            curPos += AMNVLenLen;

            int totalLength = DMJVLenLen + DMNVLenLen + AMJVLenLen + AMNVLenLen + DMJVDataLen + DMNVDataLen + AMJVDataLen + AMNVDataLen;

            return totalLength == bytes.Length;
        }
    }
}
