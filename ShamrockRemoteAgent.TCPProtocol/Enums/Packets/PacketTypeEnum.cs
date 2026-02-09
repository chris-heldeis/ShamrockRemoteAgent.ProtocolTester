namespace ShamrockRemoteAgent.TCPProtocol.Enums.Packets
{
    public enum PacketTypeEnum: byte
    {
        NOT_INITIALIZED,
        DATA_PACKET,
        NEED_WAIT,
        READY_CLOSE,
        CON_TIMEOUT,
        DATA_INVALID,
        CHECK_MASTER_STATUS
    }
}
