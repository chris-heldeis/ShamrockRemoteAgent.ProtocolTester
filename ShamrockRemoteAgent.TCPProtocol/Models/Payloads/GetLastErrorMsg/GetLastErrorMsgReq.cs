using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetLastErrorMsg
{
    public class GetLastErrorMsgReq : Payload
    {
        private static int ClientIDLen = 2;
        private static int ErrCodeLen = 2;
        public IField ClientID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ClientIDLen,
            FieldData = 0,
        };
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
                    ClientID,
                    ErrCode,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    ClientIDLen,
                    ErrCodeLen,
                ]))
                return null;

            GetLastErrorMsgReq payload = new GetLastErrorMsgReq();

            int curPos = 0;
            byte[] clientIDData = bytes[curPos..(curPos + ClientIDLen)];
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(clientIDData);
            curPos += ClientIDLen;

            byte[] errCodeData = bytes[curPos..(curPos + ErrCodeLen)];
            payload.ErrCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(errCodeData);
            curPos += ErrCodeLen;

            return payload;
        }
    }
}
