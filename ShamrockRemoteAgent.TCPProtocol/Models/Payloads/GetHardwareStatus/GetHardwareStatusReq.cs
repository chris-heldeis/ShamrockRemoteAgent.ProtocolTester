using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetHardwareStatus
{
    public class GetHardwareStatusReq : Payload
    {
        private static int ClientIDLen = 2;
        private static int InfoSizeLen = 2;
        private static int BlockOnReqLen = 2;
        public IField ClientID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ClientIDLen,
            FieldData = 0,
        };
        public IField InfoSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = InfoSizeLen,
            FieldData = 0,
        };
        public IField BlockOnReq { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = BlockOnReqLen,
            FieldData = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    ClientID,
                    InfoSize,
                    BlockOnReq,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    ClientIDLen,
                    InfoSizeLen,
                    BlockOnReqLen,
                ]))
                return null;

            GetHardwareStatusReq payload = new GetHardwareStatusReq();

            int curPos = 0;
            byte[] clientIDData = bytes[curPos..(curPos + ClientIDLen)];
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(clientIDData);
            curPos += ClientIDLen;

            byte[] infoSizeData = bytes[curPos..(curPos + InfoSizeLen)];
            payload.InfoSize.FieldData = TypeConverter.BytesToNumber<ushort>(infoSizeData);
            curPos += InfoSizeLen;

            byte[] blockOnReqData = bytes[curPos..(curPos + BlockOnReqLen)];
            payload.BlockOnReq.FieldData = TypeConverter.BytesToNumber<ushort>(blockOnReqData);
            curPos += BlockOnReqLen;

            return payload;
        }
    }
}
