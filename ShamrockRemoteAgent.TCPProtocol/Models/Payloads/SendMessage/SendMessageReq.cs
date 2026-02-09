
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage
{
    public class SendMessageReq: Payload
    {
        private static int ClientIDLen = 2;
        private static int MsgSizeLen = 2;
        private static int StatusOnTxLen = 1;
        private static int BlockOnSendLen = 1;

        public IField ClientID { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = ClientIDLen,
            FieldData = 0,
        };
        public IField MsgSize { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = MsgSizeLen,
            FieldData = 0,
        };
        public IField ClientMsg { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public IField StatusOnTx { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = StatusOnTxLen,
            FieldData = 0,
        };

        public IField BlockOnSend { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.NUMBER,
            FieldLength = BlockOnSendLen,
            FieldData = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    ClientID,
                    MsgSize,
                    ClientMsg,
                    StatusOnTx,
                    BlockOnSend,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            SendMessageReq payload = new SendMessageReq();

            int curPos = 0;
            byte[] clientIDData = bytes[curPos..(curPos + ClientIDLen)];
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(clientIDData);
            curPos += ClientIDLen;

            byte[] msgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int ClientMsgLen = TypeConverter.BytesToNumber<ushort>(msgSizeData);
            payload.MsgSize.FieldData = ClientMsgLen;
            curPos += MsgSizeLen;

            byte[] ClientMsgData = bytes[curPos..(curPos + ClientMsgLen)];
            payload.ClientMsg.FieldData = ClientMsgData;
            payload.ClientMsg.FieldLength = ClientMsgLen;
            curPos += ClientMsgLen;

            byte[] statusOnTxData = bytes[curPos..(curPos + StatusOnTxLen)];
            payload.StatusOnTx.FieldData = TypeConverter.BytesToNumber<int>(statusOnTxData);
            curPos += StatusOnTxLen;

            byte[] blockOnSendData = bytes[curPos..(curPos + BlockOnSendLen)];
            payload.BlockOnSend.FieldData = TypeConverter.BytesToNumber<int>(blockOnSendData);
            curPos += BlockOnSendLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = ClientIDLen;
            byte[] MsgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int ClientMsgLen = TypeConverter.BytesToNumber<ushort>(MsgSizeData);

            int totalLength = ClientIDLen + MsgSizeLen + ClientMsgLen + StatusOnTxLen + BlockOnSendLen;

            return totalLength == bytes.Length;
        }
    }
}
