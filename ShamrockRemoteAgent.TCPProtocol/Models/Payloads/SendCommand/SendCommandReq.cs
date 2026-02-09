using ShamrockRemoteAgent.TCPProtocol.Enums.Common;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using ShamrockRemoteAgent.TCPProtocol.Models.Fields;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendCommand
{
    public class SendCommandReq: Payload
    {
        private static int CmdNumLen = 2;
        private static int ClientIDLen = 2;
        private static int MsgSizeLen = 2;

        public IField CmdNum { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.ENUM,
            FieldLength = CmdNumLen,
            FieldData = RPCommandEnum.NOT_INITIALIZED,
        };
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
        public IField ClientCmd { get; set; } = new Field
        {
            FieldType = FieldTypeEnum.BYTE_ARRAY,
            FieldLength = 0,
        };
        public new IField[] Fields
        {
            get
            {
                return [
                    CmdNum,
                    ClientID,
                    MsgSize,
                    ClientCmd,
                ];
            }
        }
        public new static IPayload? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            SendCommandReq payload = new SendCommandReq();

            int curPos = 0;
            byte[] cmdNumData = bytes[curPos..(curPos + CmdNumLen)];
            payload.CmdNum.FieldData = TypeConverter.BytesToNumber<RPCommandEnum>(cmdNumData);
            curPos += CmdNumLen;

            byte[] clientIDData = bytes[curPos..(curPos + ClientIDLen)];
            payload.ClientID.FieldData = TypeConverter.BytesToNumber<ushort>(clientIDData);
            curPos += ClientIDLen;

            byte[] msgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int clientCmdLen = TypeConverter.BytesToNumber<ushort>(msgSizeData);
            payload.MsgSize.FieldData = clientCmdLen;
            curPos += MsgSizeLen;

            byte[] clientCmdData = bytes[curPos..(curPos + clientCmdLen)];
            payload.ClientCmd.FieldData = clientCmdData;
            payload.ClientCmd.FieldLength = clientCmdLen;
            curPos += clientCmdLen;

            return payload;
        }
        private static bool ValidateBytes(byte[] bytes)
        {
            int curPos = CmdNumLen + ClientIDLen;
            byte[] MsgSizeData = bytes[curPos..(curPos + MsgSizeLen)];
            int ClientCmdLen = TypeConverter.BytesToNumber<ushort>(MsgSizeData);

            int totalLength = CmdNumLen + ClientIDLen + MsgSizeLen + ClientCmdLen;

            return totalLength == bytes.Length;
        }
    }
}
