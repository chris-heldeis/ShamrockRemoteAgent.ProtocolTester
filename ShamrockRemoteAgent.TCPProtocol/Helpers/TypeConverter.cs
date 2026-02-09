namespace ShamrockRemoteAgent.TCPProtocol.Helpers
{
    public class TypeConverter
    {
        public static byte[] NumberToBytes<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Type t = typeof(T);

            if (t.IsEnum)
            {
                Type underlyingType = Enum.GetUnderlyingType(t);
                object underlyingValue = Convert.ChangeType(value, underlyingType);

                return underlyingType switch
                {
                    Type ut when ut == typeof(byte) => new[] { (byte)underlyingValue },
                    Type ut when ut == typeof(short) => WriteInt16BE((short)underlyingValue),
                    Type ut when ut == typeof(ushort) => WriteUInt16BE((ushort)underlyingValue),
                    Type ut when ut == typeof(int) => WriteInt32BE((int)underlyingValue),
                    Type ut when ut == typeof(uint) => WriteUInt32BE((uint)underlyingValue),
                    Type ut when ut == typeof(long) => WriteInt64BE((long)underlyingValue),
                    Type ut when ut == typeof(ulong) => WriteUInt64BE((ulong)underlyingValue),
                    _ => throw new NotSupportedException(
                        $"Enum underlying type {underlyingType.Name} is not supported.")
                };
            }

            if (t == typeof(byte))
                return [(byte)(object)value];

            if (t == typeof(short))
                return WriteInt16BE((short)(object)value);

            if (t == typeof(ushort))
                return WriteUInt16BE((ushort)(object)value);

            if (t == typeof(int))
                return WriteInt32BE((int)(object)value);

            if (t == typeof(uint))
                return WriteUInt32BE((uint)(object)value);

            if (t == typeof(long))
                return WriteInt64BE((long)(object)value);

            if (t == typeof(ulong))
                return WriteUInt64BE((ulong)(object)value);

            throw new NotSupportedException($"Type {t.Name} is not supported.");
        }

        private static byte[] WriteInt16BE(short value)
        {
            return new[]
            {
                (byte)(value >> 8),
                (byte)value
            };
        }

        private static byte[] WriteUInt16BE(ushort value)
        {
            return new[]
            {
                (byte)(value >> 8),
                (byte)value
            };
        }

        private static byte[] WriteInt32BE(int value)
        {
            return new[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        private static byte[] WriteUInt32BE(uint value)
        {
            return new[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        private static byte[] WriteInt64BE(long value)
        {
            return new[]
            {
                (byte)(value >> 56),
                (byte)(value >> 48),
                (byte)(value >> 40),
                (byte)(value >> 32),
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        private static byte[] WriteUInt64BE(ulong value)
        {
            return new[]
            {
                (byte)(value >> 56),
                (byte)(value >> 48),
                (byte)(value >> 40),
                (byte)(value >> 32),
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        public static T BytesToNumber<T>(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Type t = typeof(T);

            if (t == typeof(short))
                return (T)(object)ReadInt16BE(value);

            if (t == typeof(ushort))
                return (T)(object)ReadUInt16BE(value);

            if (t == typeof(int))
                return (T)(object)ReadInt32BE(value);

            if (t == typeof(uint))
                return (T)(object)ReadUInt32BE(value);

            if (t == typeof(long))
                return (T)(object)ReadInt64BE(value);

            if (t == typeof(ulong))
                return (T)(object)ReadUInt64BE(value);

            throw new NotSupportedException($"Type {t.Name} is not supported.");
        }
        private static short ReadInt16BE(byte[] data)
            => (short)((data[0] << 8) | data[1]);

        private static ushort ReadUInt16BE(byte[] data)
            => (ushort)((data[0] << 8) | data[1]);

        private static int ReadInt32BE(byte[] data)
            => (data[0] << 24)
             | (data[1] << 16)
             | (data[2] << 8)
             | data[3];

        private static uint ReadUInt32BE(byte[] data)
            => (uint)ReadInt32BE(data);

        private static long ReadInt64BE(byte[] data)
            => ((long)data[0] << 56)
             | ((long)data[1] << 48)
             | ((long)data[2] << 40)
             | ((long)data[3] << 32)
             | ((long)data[4] << 24)
             | ((long)data[5] << 16)
             | ((long)data[6] << 8)
             | data[7];

        private static ulong ReadUInt64BE(byte[] data)
            => (ulong)ReadInt64BE(data);
    }
}