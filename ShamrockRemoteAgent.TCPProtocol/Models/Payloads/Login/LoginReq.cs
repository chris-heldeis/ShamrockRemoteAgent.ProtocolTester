using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;
using System.Text;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Login
{
    public class LoginReq: Payload
    {
        private static int CustomerIDLen = 32;
        private static int EmailLen = 256;
        private static int DeviceIDLen = 2;
        private static int ProtocolLen = 1;
        private static int AdapterLen = 1;
        private static int ServiceTypeLen = 1;
        private static int OtherServiceLen = 256;

        public IField CustomerID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.STRING,
            FieldLength = CustomerIDLen,
            FieldData = "",
        };

        public IField Email { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.STRING,
            FieldLength = EmailLen,
            FieldData = "",
        };

        public IField DeviceID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = DeviceIDLen,
            FieldData = 0,
        };

        public IField Protocol { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ProtocolLen,
            FieldData = DeviceProtocolEnum.NOT_INITIALIZED,
        };

        public IField Adapter { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = AdapterLen,
            FieldData = DeviceAdapterEnum.NOT_INITIALIZED,
        };

        public IField ServiceType { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = ServiceTypeLen,
            FieldData = ServiceTypeEnum.NOT_INITIALIZED,
        };

        public IField OtherService { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.STRING,
            FieldLength = OtherServiceLen,
            FieldData = "",
        };

        public override IField[] Fields
        {
            get
            {
                var fields = new List<IField>
                {
                    CustomerID,
                    Email,
                    DeviceID,
                    Protocol,
                    Adapter,
                    ServiceType
                };

                if (!string.IsNullOrWhiteSpace(OtherService.FieldData as string))
                {
                    fields.Add(OtherService);
                }

                return fields.ToArray();
            }
        }


        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    CustomerIDLen,
                    EmailLen,
                    DeviceIDLen,
                    ProtocolLen,
                    AdapterLen,
                    ServiceTypeLen,
                    OtherServiceLen,
                ]))
                return null;

            LoginReq payload = new LoginReq();

            int curPos = 0;
            byte[] customerIDData = bytes[curPos..(curPos + CustomerIDLen)];
            int nullIndex = Array.IndexOf(customerIDData, 0);
            payload.CustomerID.FieldData = Encoding.UTF8.GetString(customerIDData[0..nullIndex]);
            curPos += CustomerIDLen;

            byte[] emailData = bytes[curPos..(curPos + EmailLen)];
            nullIndex = Array.IndexOf(emailData, 0);
            payload.Email.FieldData = Encoding.UTF8.GetString(emailData[0..nullIndex]);
            curPos += EmailLen;

            byte[] deviceIDData = bytes[curPos..(curPos + DeviceIDLen)];
            payload.DeviceID.FieldData = TypeConverter.BytesToNumber<ushort>(deviceIDData);
            curPos += DeviceIDLen;

            byte[] protocolData = bytes[curPos..(curPos + ProtocolLen)];
            payload.Protocol.FieldData = TypeConverter.BytesToNumber<DeviceProtocolEnum>(protocolData);
            curPos += ProtocolLen;

            byte[] adapterData = bytes[curPos..(curPos + AdapterLen)];
            payload.Adapter.FieldData = TypeConverter.BytesToNumber<DeviceAdapterEnum>(adapterData);
            curPos += AdapterLen;

            byte[] serviceTypeData = bytes[curPos..(curPos + ServiceTypeLen)];
            payload.ServiceType.FieldData = TypeConverter.BytesToNumber<ServiceTypeEnum>(serviceTypeData);
            curPos += ServiceTypeLen;

            byte[] otherServiceData = bytes[curPos..];
            nullIndex = Array.IndexOf(otherServiceData, 0);
            payload.OtherService.FieldData = Encoding.UTF8.GetString(otherServiceData[0..nullIndex]);

            return payload;
        }
    }
}
