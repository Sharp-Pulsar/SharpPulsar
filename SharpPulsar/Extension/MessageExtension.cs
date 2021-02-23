using System.Buffers;
using System.Linq;
using SharpPulsar.Helpers;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Extension
{
    public static class MessageExtension
    {

        //public static bool IsValid(this ReadOnlySequence<byte> data)
           // => StartsWithMagicNumber(data) && HasValidCheckSum(data);

        public static bool StartsWithMagicNumber(this ReadOnlySequence<byte> input)
            => input.StartsWith(Constants.MagicNumber);

        public static bool HasValidCheckSum(this ReadOnlySequence<byte> input)
            => input.ReadUInt32(Constants.MagicNumber.Length, true) == DotCrc32C.Calculate(input.Slice(Constants.MetadataSizeOffset));

        public static bool IsNewCommand(this ReadOnlySequence<byte> buffer)
        {
            var cmd = buffer.ToArray().Take(3).ToArray();
            foreach(var b in cmd)
            {
                if (b != 0)
                    return false;
            }
            return true;
        }
    }
}
