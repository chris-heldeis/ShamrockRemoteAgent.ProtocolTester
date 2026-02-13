using ShamrockRemoteAgent.ClientTester.Models;

namespace ShamrockRemoteAgent.ClientTester.Services
{
    public static class PacketBus
    {
        public static event Action<byte[]>? PacketBuilt;
        public static event Action<string>? LogPublished;
        public static event Action<DecodedPacket>? PacketDecoded;
        public static event Action<BrokerDecodedPacket>? BrokerPacketDecoded;

        private static readonly List<byte[]> _packetHistory = new();
        private static readonly List<string> _logHistory = new();
        private static readonly List<DecodedPacket> _decodedHistory = new();
        private static readonly List<BrokerDecodedPacket> _brokerDecodedHistory = new();

        public static IReadOnlyList<byte[]> PacketHistory => _packetHistory;
        public static IReadOnlyList<string> LogHistory => _logHistory;
        public static IReadOnlyList<DecodedPacket> DecodedHistory => _decodedHistory;
        public static IReadOnlyList<BrokerDecodedPacket> BrokerDecodedHistory => _brokerDecodedHistory;

        public static void Publish(byte[] packet)
        {
            _packetHistory.Add(packet);
            PacketBuilt?.Invoke(packet);
        }

        public static void PublishLog(string message)
        {
            _logHistory.Add(message);
            LogPublished?.Invoke(message);
        }

        public static void PublishDecoded(DecodedPacket packet)
        {
            _decodedHistory.Add(packet);
            PacketDecoded?.Invoke(packet);
        }

        public static void PublishBrokerDecoded(BrokerDecodedPacket packet)
        {
            _brokerDecodedHistory.Add(packet);
            BrokerPacketDecoded?.Invoke(packet);
        }

        // Optional: clear history when reconnecting
        public static void Clear()
        {
            _packetHistory.Clear();
            _logHistory.Clear();
            _decodedHistory.Clear();
            _brokerDecodedHistory.Clear();
        }
    }
}
