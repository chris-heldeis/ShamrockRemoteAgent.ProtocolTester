using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg
{
    public class GetErrorMsgRes: Payload
    {
        private static int ResultCodeLen = 2;
        private static int ErrDescSizeLen = 1;
        public IField ResultCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ResultCodeLen,
            FieldData = RPErrorCodeEnum.NO_ERRORS,
        };
        public IField ErrDescSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ErrDescSizeLen,
            FieldData = 0,
        };
        public IField ErrDesc { get; set; } = new Field
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
                    ErrDescSize,
                    ErrDesc,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            GetErrorMsgRes payload = new GetErrorMsgRes();

            int curPos = 0;
            byte[] resultCodeData = bytes[curPos..(curPos + ResultCodeLen)];
            payload.ResultCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(resultCodeData);
            curPos += ResultCodeLen;

            byte[] errDescSizeData = bytes[curPos..(curPos + ErrDescSizeLen)];
            int ErrDescLen = TypeConverter.BytesToNumber<ushort>(errDescSizeData);
            payload.ErrDescSize.FieldData = ErrDescLen;
            curPos += ErrDescLen;

            byte[] errDescData = bytes[curPos..(curPos + ErrDescLen)];
            payload.ErrDesc.FieldData = errDescData;
            payload.ErrDesc.FieldLength = ErrDescLen;
            curPos += ErrDescLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = ResultCodeLen;
            byte[] errDescSizeData = bytes[curPos..(curPos + ErrDescSizeLen)];
            int ErrDescLen = TypeConverter.BytesToNumber<ushort>(errDescSizeData);

            int totalLength = ResultCodeLen + ErrDescSizeLen + ErrDescLen;

            return totalLength == bytes.Length;
        }
    }
}
