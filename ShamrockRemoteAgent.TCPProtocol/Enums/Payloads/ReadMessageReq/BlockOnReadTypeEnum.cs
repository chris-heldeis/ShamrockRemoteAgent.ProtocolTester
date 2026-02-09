namespace ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.ReadMessageReq
{
    public enum BlockOnReadTypeEnum: byte
    {
        NOT_INITIALIZED,
        BLOCKING_IO,
        NON_BLOCKING_IO
    }
}
