using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads;

namespace ShamrockRemoteAgent.TCPProtocol.Interfaces
{
    public interface IField
    {
        FieldTypeEnum FieldType { get; set; }
        Int32 FieldLength { get; set; }
        dynamic? FieldData { get; set; }
        abstract public Int32 GetLength();
        abstract public byte[] Serialize();
    }
}
