using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadDetailedVersion
{
    public class ReadDetailedVersionRes : Payload
    {
        private static int ResultCodeLen = 2;
        private static int AVISizeLen = 1;
        private static int DVISizeLen = 1;
        private static int FVISizeLen = 1;
        public IField ResultCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ResultCodeLen,
            FieldData = RPErrorCodeEnum.NO_ERRORS,
        };
        public IField AVISize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = AVISizeLen,
            FieldData = 0,
        };
        public IField DVISize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = DVISizeLen,
            FieldData = 0,
        };
        public IField FVISize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = FVISizeLen,
            FieldData = 0,
        };
        public IField AVI { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField DVI { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField FVI { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    ResultCode,
                    AVISize,
                    DVISize,
                    FVISize,
                    AVI,
                    DVI,
                    FVI,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            ReadDetailedVersionRes payload = new ReadDetailedVersionRes();

            int curPos = 0;
            byte[] resultCodeData = bytes[curPos..(curPos + ResultCodeLen)];
            payload.ResultCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(resultCodeData);
            curPos += ResultCodeLen;

            byte[] AVISizeData = bytes[curPos..(curPos + AVISizeLen)];
            int AVILen = TypeConverter.BytesToNumber<ushort>(AVISizeData);
            payload.AVISize.FieldData = AVILen;
            curPos += AVISizeLen;

            byte[] DVISizeData = bytes[curPos..(curPos + DVISizeLen)];
            int DVILen = TypeConverter.BytesToNumber<ushort>(DVISizeData);
            payload.DVISize.FieldData = DVILen;
            curPos += DVISizeLen;

            byte[] FVISizeData = bytes[curPos..(curPos + FVISizeLen)];
            int FVILen = TypeConverter.BytesToNumber<ushort>(FVISizeData);
            payload.FVISize.FieldData = FVILen;
            curPos += FVISizeLen;

            byte[] AVIData = bytes[curPos..(curPos + AVILen)];
            payload.AVI.FieldData = TypeConverter.BytesToNumber<ushort>(AVIData);
            payload.AVI.FieldLength = AVILen;
            curPos += AVILen;

            byte[] DVIData = bytes[curPos..(curPos + DVILen)];
            payload.DVI.FieldData = TypeConverter.BytesToNumber<ushort>(DVIData);
            payload.DVI.FieldLength = DVILen;
            curPos += DVILen;

            byte[] FVIData = bytes[curPos..(curPos + FVILen)];
            payload.FVI.FieldData = TypeConverter.BytesToNumber<ushort>(FVIData);
            payload.FVI.FieldLength = FVILen;
            curPos += FVILen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = ResultCodeLen + AVISizeLen + DVISizeLen + FVISizeLen;

            byte[] AVIData = bytes[curPos..(curPos + AVISizeLen)];
            int AVIDataLen = TypeConverter.BytesToNumber<ushort>(AVIData);
            curPos += AVISizeLen;

            byte[] DVIData = bytes[curPos..(curPos + DVISizeLen)];
            int DVIDataLen = TypeConverter.BytesToNumber<ushort>(DVIData);
            curPos += DVISizeLen;

            byte[] FVIData = bytes[curPos..(curPos + FVISizeLen)];
            int FVIDataLen = TypeConverter.BytesToNumber<ushort>(FVIData);
            curPos += FVISizeLen;

            int totalLength = ResultCodeLen + AVISizeLen + DVISizeLen + FVISizeLen + AVIDataLen + DVIDataLen + FVIDataLen;

            return totalLength == bytes.Length;
        }
    }
}
