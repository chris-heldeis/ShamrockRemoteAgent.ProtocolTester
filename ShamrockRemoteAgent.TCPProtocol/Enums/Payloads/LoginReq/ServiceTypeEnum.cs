namespace ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq
{
    public enum ServiceTypeEnum: byte
    {
        NOT_INITIALIZED,
        CALIBRATION_UPDATE,
        DTC_REMOVAL,
        EMISSIONS_TUNING,
        HORSEPOWER_UPDATE,
        OTHER_SERVICE,
    }
}
