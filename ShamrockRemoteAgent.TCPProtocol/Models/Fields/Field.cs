using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;
using System.Text;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Fields
{
    public class Field: IField
    {
        public FieldTypeEnum FieldType { get; set; }
        public Int32 FieldLength { get; set; }
        public dynamic? FieldData { get; set; }

        public Int32 GetLength()
        {
            if (FieldType == FieldTypeEnum.ENUM || 
                FieldType == FieldTypeEnum.NUMBER ||
                FieldType == FieldTypeEnum.STRING)
                return FieldLength;

            return (FieldData! as byte[])!.Length;
        }

        public byte[] Serialize()
        {
            if (FieldType == FieldTypeEnum.ENUM ||
                FieldType == FieldTypeEnum.NUMBER)
                return TypeConverter.NumberToBytes(FieldData);

            if (FieldType == FieldTypeEnum.STRING)
            {
                byte[] strBytes = Encoding.UTF8.GetBytes((FieldData as string)!);
                if (FieldLength > 0)
                {
                    byte[] bytes = new byte[FieldLength];
                    Array.Copy(strBytes, 0, bytes, 0, strBytes.Length);
                    return bytes;
                }
                else
                    return strBytes;
            }

            if (FieldType == FieldTypeEnum.BYTE_ARRAY)
            {
                byte[] dataArray = (FieldData as byte[])!;
                if (FieldLength > 0)
                {
                    byte[] bytes = new byte[FieldLength];
                    Array.Copy(dataArray, 0, bytes, 0, dataArray.Length);
                    return bytes;
                }
                else
                    return dataArray;
            }

            if (FieldType == FieldTypeEnum.SBYTE_ARRAY)
            {
                sbyte[] dataArray = (FieldData as sbyte[])!;
                byte[] byteArray = dataArray.Select(x => (byte)(x + 128)).ToArray();
                if (FieldLength > 0)
                {
                    byte[] bytes = new byte[FieldLength];
                    Array.Copy(byteArray, 0, bytes, 0, byteArray.Length);
                    return bytes;
                }
                else
                    return byteArray;
            }

            throw new NotSupportedException($"Field type ${FieldType.GetType().Name} is not supported.");
        }
    }
}
