using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadDetailedVersion
{
    public class ReadDetailedVersionReq : Payload
    {
        private static int ClientIDLen = 2;

        public IField ClientID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ClientIDLen,
            FieldData = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    ClientID,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    ClientIDLen,
                ]))
                return null;

            ReadDetailedVersionReq payload = new ReadDetailedVersionReq();

            byte[] ClientIDData = bytes;
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(ClientIDData);

            return payload;
        }
    }
}
