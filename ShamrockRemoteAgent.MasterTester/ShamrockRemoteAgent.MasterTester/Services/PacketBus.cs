using ShamrockRemoteAgent.MasterTester.Models;

namespace ShamrockRemoteAgent.MasterTester.Services;

public static class PacketBus
{
    public static event Action<byte[]>? PacketBuilt;
    public static event Action<string>? LogPublished;
    public static event Action<DecodedPacket>? PacketDecoded;
    public static event Action<BrokerDecodedPacket>? BrokerPacketDecoded;

    public static void Publish(byte[] packet)
    {
        PacketBuilt?.Invoke(packet);
    }

    public static void PublishLog(string message)
    {
        LogPublished?.Invoke(message);
    }
    public static void PublishDecoded(DecodedPacket packet)
    {
        PacketDecoded?.Invoke(packet);
    }
    public static void PublishBrokerDecoded(BrokerDecodedPacket packet)
    {
        BrokerPacketDecoded?.Invoke(packet);
    }
}
