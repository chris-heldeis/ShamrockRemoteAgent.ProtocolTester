using System.Text;

namespace ShamrockRemoteAgent.MasterTester.Helpers
{
    public static class MessageConverter
    {
        /// <summary>
        /// Converts user input string to byte array.
        /// Supports HEX format ("01 0A FF") or plain text.
        /// </summary>
        public static byte[] StringToBytes(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<byte>();

            input = input.Trim();

            // Detect HEX format: "01 0A FF"
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            bool isHex = parts.Length > 1 &&
                         parts.All(p =>
                             p.Length <= 2 &&
                             p.All(c => Uri.IsHexDigit(c)));

            if (isHex)
            {
                byte[] bytes = new byte[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                    bytes[i] = Convert.ToByte(parts[i], 16);

                return bytes;
            }

            // Otherwise treat as UTF8 text
            return Encoding.UTF8.GetBytes(input);
        }

        /// <summary>
        /// Converts byte array to string.
        /// If asHex = true → returns HEX format.
        /// Otherwise returns UTF8 string.
        /// </summary>
        public static string BytesToString(byte[] bytes, bool asHex = false)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            if (asHex)
            {
                return BitConverter.ToString(bytes).Replace("-", " ");
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
