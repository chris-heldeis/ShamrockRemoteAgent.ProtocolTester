using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadMessage
{
    public class ReadMessageRes: Payload
    {
        private static int ResultCodeLen = 2;
        private static int MsgSizeLen = 2;

        public IField ResultCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ResultCodeLen,
            FieldData = RPErrorCodeEnum.NO_ERRORS,
        };
        public IField MsgSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = MsgSizeLen,
            FieldData = 0,
        };
        public IField ApiMsg { get; set; } = new Field
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
                    MsgSize,
                    ApiMsg,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            ReadMessageRes payload = new ReadMessageRes();

            int curPos = 0;
            byte[] resultCodeData = bytes[curPos..(curPos + ResultCodeLen)];
            payload.ResultCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(resultCodeData);
            curPos += ResultCodeLen;

            byte[] msgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int ApiMsgLen = TypeConverter.BytesToNumber<ushort>(msgSizeData);
            payload.MsgSize.FieldData = ApiMsgLen;
            curPos += MsgSizeLen;

            byte[] apiMsgData = bytes[curPos..(curPos + ApiMsgLen)];
            payload.ApiMsg.FieldData = apiMsgData;
            payload.ApiMsg.FieldLength = ApiMsgLen;
            curPos += ApiMsgLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = ResultCodeLen;
            byte[] MsgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int ApiMsgLen = TypeConverter.BytesToNumber<ushort>(MsgSizeData);

            int totalLength = ResultCodeLen + MsgSizeLen + ApiMsgLen;

            return totalLength == bytes.Length;
        }
    }
}
