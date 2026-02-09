using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg
{
    public class GetErrorMsgReq : Payload
    {
        private static int ErrCodeLen = 2;
        public IField ErrCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ErrCodeLen,
            FieldData = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    ErrCode,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    ErrCodeLen,
                ]))
                return null;

            GetErrorMsgReq payload = new GetErrorMsgReq();
            byte[] ErrCodeData = bytes;
            payload.ErrCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(ErrCodeData);

            return payload;
        }
    }
}
