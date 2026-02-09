namespace ShamrockRemoteAgent.ClientTester.Helpers
{
    public static class HexFormatter
    {
        public static string ToHex(byte[] data)
        {
            return string.Join(" ",
                data.Select(b => b.ToString("X2")));
        }
    }
}
