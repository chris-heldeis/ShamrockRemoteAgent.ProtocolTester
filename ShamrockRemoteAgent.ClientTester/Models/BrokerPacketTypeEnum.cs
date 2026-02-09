namespace ShamrockRemoteAgent.ClientTester.Models
{
    public enum BrokerPacketTypeEnum: byte
    {
        COM_DATA = 0x01,
        NEED_WAIT = 0x02,
        READY_CLOSE = 0x03,
        CON_TIMEOUT = 0x04,
        DATA_INVALID = 0x05
    }
}
