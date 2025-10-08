//https://github.com/eaba/EasyTestSocket.git
using System.Runtime.CompilerServices;

namespace SharpPulsar.Shared.Buf;

public static class ByteUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetByte(byte[] memory, int index) => memory[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetShort(byte[] memory, int index) =>
        unchecked((short)(memory[index] << 8 | memory[index + 1]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetShortLE(byte[] memory, int index) =>
        unchecked((short)(memory[index] | memory[index + 1] << 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetUShort(byte[] memory, int index) =>
        unchecked((ushort)(memory[index] << 8 | memory[index + 1]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetUShortLE(byte[] memory, int index) =>
        unchecked((ushort)(memory[index] | memory[index + 1] << 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUnsignedMedium(byte[] memory, int index) =>
        unchecked(
            memory[index] << 16 |
            memory[index + 1] << 8 |
            memory[index + 2]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUnsignedMediumLE(byte[] memory, int index) =>
        unchecked(
            memory[index] |
            memory[index + 1] << 8 |
            memory[index + 2] << 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt(byte[] memory, int index) =>
        unchecked(
            memory[index] << 24 |
            memory[index + 1] << 16 |
            memory[index + 2] << 8 |
            memory[index + 3]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIntLE(byte[] memory, int index) =>
        unchecked(
            memory[index] |
            memory[index + 1] << 8 |
            memory[index + 2] << 16 |
            memory[index + 3] << 24);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetLong(byte[] memory, int index) =>
        unchecked(
            (long)memory[index] << 56 |
            (long)memory[index + 1] << 48 |
            (long)memory[index + 2] << 40 |
            (long)memory[index + 3] << 32 |
            (long)memory[index + 4] << 24 |
            (long)memory[index + 5] << 16 |
            (long)memory[index + 6] << 8 |
            memory[index + 7]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetLongLE(byte[] memory, int index) =>
        unchecked(
            memory[index] |
            (long)memory[index + 1] << 8 |
            (long)memory[index + 2] << 16 |
            (long)memory[index + 3] << 24 |
            (long)memory[index + 4] << 32 |
            (long)memory[index + 5] << 40 |
            (long)memory[index + 6] << 48 |
            (long)memory[index + 7] << 56);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetByte(byte[] memory, int index, int value)
    {
        unchecked
        {
            memory[index] = (byte)value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetShort(byte[] memory, int index, int value)
    {
        unchecked
        {
            memory[index] = (byte)((ushort)value >> 8);
            memory[index + 1] = (byte)value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetShortLE(byte[] memory, int index, int value)
    {
        unchecked
        {
            memory[index] = (byte)value;
            memory[index + 1] = (byte)((ushort)value >> 8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetMedium(byte[] memory, int index, int value)
    {
        unchecked
        {
            uint unsignedValue = (uint)value;
            memory[index] = (byte)(unsignedValue >> 16);
            memory[index + 1] = (byte)(unsignedValue >> 8);
            memory[index + 2] = (byte)unsignedValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetMediumLE(byte[] memory, int index, int value)
    {
        unchecked
        {
            uint unsignedValue = (uint)value;
            memory[index] = (byte)unsignedValue;
            memory[index + 1] = (byte)(unsignedValue >> 8);
            memory[index + 2] = (byte)(unsignedValue >> 16);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetInt(byte[] memory, int index, int value)
    {
        unchecked
        {
            uint unsignedValue = (uint)value;
            memory[index] = (byte)(unsignedValue >> 24);
            memory[index + 1] = (byte)(unsignedValue >> 16);
            memory[index + 2] = (byte)(unsignedValue >> 8);
            memory[index + 3] = (byte)unsignedValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIntLE(byte[] memory, int index, int value)
    {
        unchecked
        {
            uint unsignedValue = (uint)value;
            memory[index] = (byte)unsignedValue;
            memory[index + 1] = (byte)(unsignedValue >> 8);
            memory[index + 2] = (byte)(unsignedValue >> 16);
            memory[index + 3] = (byte)(unsignedValue >> 24);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLong(byte[] memory, int index, long value)
    {
        unchecked
        {
            ulong unsignedValue = (ulong)value;
            memory[index] = (byte)(unsignedValue >> 56);
            memory[index + 1] = (byte)(unsignedValue >> 48);
            memory[index + 2] = (byte)(unsignedValue >> 40);
            memory[index + 3] = (byte)(unsignedValue >> 32);
            memory[index + 4] = (byte)(unsignedValue >> 24);
            memory[index + 5] = (byte)(unsignedValue >> 16);
            memory[index + 6] = (byte)(unsignedValue >> 8);
            memory[index + 7] = (byte)unsignedValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLongLE(byte[] memory, int index, long value)
    {
        unchecked
        {
            var unsignedValue = (ulong)value;
            memory[index] = (byte)unsignedValue;
            memory[index + 1] = (byte)(unsignedValue >> 8);
            memory[index + 2] = (byte)(unsignedValue >> 16);
            memory[index + 3] = (byte)(unsignedValue >> 24);
            memory[index + 4] = (byte)(unsignedValue >> 32);
            memory[index + 5] = (byte)(unsignedValue >> 40);
            memory[index + 6] = (byte)(unsignedValue >> 48);
            memory[index + 7] = (byte)(unsignedValue >> 56);
        }
    }
}
