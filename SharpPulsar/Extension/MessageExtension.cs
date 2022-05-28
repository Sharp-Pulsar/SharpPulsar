using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Extension
{
    public static class MessageExtension
    {

        //public static bool IsValid(this ReadOnlySequence<byte> data)
           // => StartsWithMagicNumber(data) && HasValidCheckSum(data);
/*
        public static bool StartsWithMagicNumber(this ReadOnlySequence<byte> input)
            => input.StartsWith(Constants.MagicNumber);

        public static bool HasValidCheckSum(this ReadOnlySequence<byte> input)
            => input.ReadUInt32(Constants.MagicNumber.Length, true) == DotCrc32C.Calculate(input.Slice(Constants.MetadataSizeOffset));
        */
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
        public static long[] ToLongArray(this BitArray bitSet)
        {
            var longs = new long[bitSet.Length];
            var resultArrayLengthLongs = bitSet.Length / 64 + (bitSet.Length % 64 == 0? 0: 1);
            var resultArray = Array.CreateInstance(typeof(byte), resultArrayLengthLongs * 8);
            bitSet.CopyTo(resultArray, 0);
            resultArray.CopyTo(longs, 0);
            return longs;
        }
        public static void Empty<T>(this BufferBlock<IMessage<T>> messages)
        {
            if (messages.TryReceiveAll(out var m))
            {
                return;
            }
        }
        public static BitArray FromLongArray(this IList<long> ackSets, int numMessagesInBatch)
        {
            var bitArray = new BitArray(numMessagesInBatch);
            var index = 0;
            foreach(var ackSet in ackSets)
            {
                var stillToGo = numMessagesInBatch - index;
                var currentLimit = stillToGo > 64 ? 64 : stillToGo;
                for(var i = 0; i <= currentLimit; i++)
                {
                    bitArray[index] = (ackSet & (1 << i - 1)) != 0;
                    index = index + 1;
                }
                
            }
            return bitArray;
        }
    }
}
