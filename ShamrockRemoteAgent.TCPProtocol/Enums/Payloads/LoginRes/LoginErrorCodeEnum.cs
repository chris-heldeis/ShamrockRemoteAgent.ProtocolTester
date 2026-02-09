namespace ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginRes
{
    public enum LoginErrorCodeEnum: byte
    {
        NO_ERROR,
        INVALID_CLIENT_ID,
        INVALID_EMAIL,
        INVALID_ADAPTER,
        INVALID_SERVICE,
        CONTACT_TECHNICIAN
    }
}
