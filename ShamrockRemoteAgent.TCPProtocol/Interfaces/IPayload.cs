namespace ShamrockRemoteAgent.TCPProtocol.Interfaces
{
    public interface IPayload
    {
        IField[] Fields { get; }
        abstract public byte[] Serialize();
        abstract public static IPayload? Deserialize(byte[] bytes);
    }
}
