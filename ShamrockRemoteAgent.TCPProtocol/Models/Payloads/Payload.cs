using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads
{
    public class Payload: IPayload
    {
        public virtual IField[] Fields { get; } = [];

        public static bool ValidateBytes(byte[] bytes, int[] arrayLength)
        {
            int totalLength = 0;
            foreach (int fieldLength in arrayLength)
            {
                totalLength += fieldLength;
            }

            return totalLength == bytes.Length;
        }

        private bool ValidateSelf()
        {
            bool valid = true;
            foreach (IField field in Fields)
            {
                if (field.FieldType == FieldTypeEnum.ENUM &&
                    field.FieldData == 0) // NOT_INITIALIZED
                {
                    valid = false;
                    break;
                }

                if (field.FieldType == FieldTypeEnum.STRING &&
                    string.IsNullOrEmpty(field.FieldData))
                {
                    valid = false;
                    break;
                }

                if ((field.FieldType == FieldTypeEnum.SBYTE_ARRAY ||
                    field.FieldType == FieldTypeEnum.BYTE_ARRAY) &&
                    (field.FieldData as byte[])!.Length == 0)
                {
                    valid = false;
                    break;
                }
            }

            return valid;
        }

        public byte[] Serialize()
        {
            if (!ValidateSelf())
                return [];

            int totalLength = Fields.Sum(f => f.GetLength());
            byte[] serialized = new byte[totalLength];

            int curPos = 0;
            foreach (IField field in Fields)
            {
                byte[] fieldBytes = field.Serialize();
                Buffer.BlockCopy(fieldBytes, 0, serialized, curPos, fieldBytes.Length);
                curPos += fieldBytes.Length;
            }

            return serialized;
        }
        public static IPayload? Deserialize(byte[] bytes)
        {
            return null;
        }
    }
}
