using System;
using System.Runtime.InteropServices;

namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.VariableWidthBlockBuilder.java
    /// 
    /// TODO: Finish implementation
    /// </summary>
    public class VariableWidthBlockBuilder : IBlockBuilder
    {
        #region Private Fields

        private int _InitialEntryCount;

        private bool _Initialized;

        private int _InitialSliceOutputSize;

        private bool[] _ValueIsNull = new bool[0];

        private int[] _Offsets = new int[1];

        private long _ArraysRetainedSizeInBytes;

        private int _CurrentEntrySize;

        private SliceOutput _SliceOutput; // = new DynamicSliceOutput(0);

        #endregion

        #region Public Properties

        public BlockBuilderStatus BlockBuilderStatus { get; }

        public int PositionCount { get; }

        #endregion

        #region Constructors

        public VariableWidthBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries, int expectedBytes)
        {
            BlockBuilderStatus = blockBuilderStatus;

            _InitialEntryCount = expectedEntries;

            _InitialSliceOutputSize = Math.Min(expectedBytes, BlockUtil.MaxArraySize);

            UpdateArraysDataSize();
        }

        #endregion


        #region Public Methods

        public IBlockBuilder WriteByte(int value)
        {
            /*
            if (!this._Initialized)
            {
                this.InitializeCapacity();
            }

            this._SliceOutput.WriteByte(value);
            this._CurrentEntrySize += SIZE_OF_BYTE;
            return this;
            */
            throw new NotImplementedException();
        }

        public IBlockBuilder WriteShort(int value)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder WriteInt(int value)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder WriteLong(long value)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder WriteBytes(Slice source, int sourceIndex, int length)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder WriteObject(object value)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder BeginBlockEntry()
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder GetPositions(int[] visiblePositions, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder CloseEntry()
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder AppendNull()
        {
            throw new NotImplementedException();
        }

        public IBlock Build()
        {
            throw new NotImplementedException();
        }

        public IBlockBuilder NewBlockBuilderLike(BlockBuilderStatus blockBuilderStatus)
        {
            throw new NotImplementedException();
        }

        public int GetSliceLength(int position)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int position, int offset)
        {
            throw new NotImplementedException();
        }

        public short GetShort(int position, int offset)
        {
            throw new NotImplementedException();
        }

        public int GetInt(int position, int offset)
        {
            throw new NotImplementedException();
        }

        public long GetLong(int position, int offset)
        {
            throw new NotImplementedException();
        }

        public Slice GetSlice(int position, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public object GetObject(int position, System.Type type)
        {
            throw new NotImplementedException();
        }

        public bool BytesEqual(int position, int offset, Slice otherSlice, int otherOffset, int length)
        {
            throw new NotImplementedException();
        }

        public int BytesCompare(int position, int offset, int length, Slice otherSlice, int otherOffset, int otherLength)
        {
            throw new NotImplementedException();
        }

        public void WriteBytesTo(int position, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void WritePositionTo(int position, IBlockBuilder blockBuilder)
        {
            throw new NotImplementedException();
        }

        public bool Equals(int position, int offset, IBlock otherBlock, int otherPosition, int otherOffset, int length)
        {
            throw new NotImplementedException();
        }

        public long Hash(int position, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(int leftPosition, int leftOffset, int leftLength, IBlock rightBlock, int rightPosition, int rightOffset, int rightLength)
        {
            throw new NotImplementedException();
        }

        public IBlock GetSingleValueBlock(int position)
        {
            throw new NotImplementedException();
        }

        public int GetPositionCount()
        {
            throw new NotImplementedException();
        }

        public long GetSizeInBytes()
        {
            throw new NotImplementedException();
        }

        public long GetRegionSizeInBytes(int position, int length)
        {
            throw new NotImplementedException();
        }

        public long GetRetainedSizeInBytes()
        {
            throw new NotImplementedException();
        }

        public void RetainedBytesForEachPart(BiConsumer<object, long> consumer)
        {
            throw new NotImplementedException();
        }

        public IBlockEncoding GetEncoding()
        {
            throw new NotImplementedException();
        }

        public IBlock CopyPositions(int[] positions, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public IBlock GetRegion(int positionOffset, int length)
        {
            throw new NotImplementedException();
        }

        public IBlock CopyRegion(int position, int length)
        {
            throw new NotImplementedException();
        }

        public bool IsNull(int position)
        {
            throw new NotImplementedException();
        }

        public void AssureLoaded()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private void UpdateArraysDataSize()
        {
            _ArraysRetainedSizeInBytes = Marshal.SizeOf(_ValueIsNull) + Marshal.SizeOf(_Offsets);
        }

        private void InitializeCapacity()
        {
            if (PositionCount != 0 || _CurrentEntrySize != 0)
            {
                throw new InvalidOperationException($"{GetType().Name} was used before initialization");
            }

            _Initialized = true;
            _ValueIsNull = new bool[_InitialEntryCount];
            _Offsets = new int[_InitialEntryCount + 1];
            _SliceOutput = null; // new DynamicSliceOutput(this._InitialSliceOutputSize);
            UpdateArraysDataSize();
        }

        #endregion
    }
}
