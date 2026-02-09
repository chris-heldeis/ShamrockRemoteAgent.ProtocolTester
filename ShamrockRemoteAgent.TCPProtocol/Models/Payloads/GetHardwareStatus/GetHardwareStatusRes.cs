using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetHardwareStatus
{
    public class GetHardwareStatusRes : Payload
    {
        private static int ResultCodeLen = 2;
        private static int HwStatusSizeLen = 2;
        public IField ResultCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ResultCodeLen,
            FieldData = RPErrorCodeEnum.NO_ERRORS,
        };
        public IField HwStatusSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = HwStatusSizeLen,
            FieldData = 0,
        };
        public IField HwStatus { get; set; } = new Field
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
                    HwStatusSize,
                    HwStatus,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            GetHardwareStatusRes payload = new GetHardwareStatusRes();

            int curPos = 0;
            byte[] resultCodeData = bytes[curPos..(curPos + ResultCodeLen)];
            payload.ResultCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(resultCodeData);
            curPos += ResultCodeLen;

            byte[] hwStatusSizeData = bytes[curPos..(curPos + HwStatusSizeLen)];
            int HwStatusLen = TypeConverter.BytesToNumber<ushort>(hwStatusSizeData);
            payload.HwStatusSize.FieldData = HwStatusLen;
            curPos += HwStatusLen;

            byte[] hwStatusData = bytes[curPos..(curPos + HwStatusLen)];
            payload.HwStatus.FieldData = hwStatusData;
            payload.HwStatus.FieldLength = HwStatusLen;
            curPos += HwStatusLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = ResultCodeLen;
            byte[] HwStatusSizeData = bytes[curPos..(curPos + HwStatusSizeLen)];
            int HwStatusLen = TypeConverter.BytesToNumber<ushort>(HwStatusSizeData);

            int totalLength = ResultCodeLen + HwStatusSizeLen + HwStatusLen;

            return totalLength == bytes.Length;
        }
    }
}
