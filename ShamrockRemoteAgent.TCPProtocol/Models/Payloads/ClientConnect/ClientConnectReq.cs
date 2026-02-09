using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ClientConnectReq;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientConnect
{
    public class ClientConnectReq : Payload
    {
        private static int DeviceIDLen = 2;
        private static int ProtocolLen = 1;
        private static int TxBufSizeLen = 4;
        private static int RxBufSizeLen = 4;
        private static int SelfPacketizeLen = 1;

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

        public IField TxBufSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = TxBufSizeLen,
            FieldData = 0,
        };

        public IField RxBufSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = RxBufSizeLen,
            FieldData = 0,
        };

        public IField SelfPacketize { get; set; } = new Field { 
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = SelfPacketizeLen,
            FieldData = SelfPacketizeEnum.NOT_INITIALIZED,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    DeviceID,
                    Protocol,
                    TxBufSize,
                    RxBufSize,
                    SelfPacketize,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes, [
                    DeviceIDLen,
                    ProtocolLen,
                    TxBufSizeLen,
                    RxBufSizeLen,
                    SelfPacketizeLen,
                ]))
                return null;

            ClientConnectReq payload = new ClientConnectReq();

            int curPos = 0;
            byte[] deviceIDData = bytes[curPos..(curPos + DeviceIDLen)];
            payload.DeviceID.FieldData = TypeConverter.BytesToNumber<ushort>(deviceIDData);
            curPos += DeviceIDLen;

            byte[] protocolData = bytes[curPos..(curPos + ProtocolLen)];
            payload.Protocol.FieldData = TypeConverter.BytesToNumber<DeviceProtocolEnum>(protocolData);
            curPos += ProtocolLen;

            byte[] txBufSizeData = bytes[curPos..(curPos + TxBufSizeLen)];
            payload.TxBufSize.FieldData = TypeConverter.BytesToNumber<ushort>(txBufSizeData);
            curPos += TxBufSizeLen;

            byte[] rxBufSizeData = bytes[curPos..(curPos + RxBufSizeLen)];
            payload.RxBufSize.FieldData = TypeConverter.BytesToNumber<ushort>(rxBufSizeData);
            curPos += RxBufSizeLen;

            byte[] selfPacketizeData = bytes[curPos..(curPos + SelfPacketizeLen)];
            payload.SelfPacketize.FieldData = TypeConverter.BytesToNumber<DeviceProtocolEnum>(selfPacketizeData);
            curPos += SelfPacketizeLen;

            return payload;
        }
    }
}
