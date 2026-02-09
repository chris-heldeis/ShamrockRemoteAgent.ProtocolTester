using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand
{
    public class SendCommandRes: Payload
    {
        private static int ResultCodeLen = 2;

        public IField ResultCode { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ResultCodeLen,
            FieldData = RPErrorCodeEnum.NO_ERRORS,
        };

        public new IField[] Fields
        {
            get
            {
                return [
                    ResultCode
                ];
            }
        }

        public new static IPayload? Deserialize(byte[] bytes)
        {
            SendCommandRes payload = new SendCommandRes();
            byte[] resultCodeData = bytes;
            payload.ResultCode.FieldData = TypeConverter.BytesToNumber<RPErrorCodeEnum>(resultCodeData);

            return payload;
        }
    }
}
