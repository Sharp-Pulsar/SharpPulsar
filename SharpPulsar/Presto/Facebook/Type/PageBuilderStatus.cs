namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.PageBuilderStatus.java
    /// </summary>
    public class PageBuilderStatus
    {
        #region Private Fields

        private bool _full;

        private int _currentSize;

        #endregion

        #region Public Fields

        public static readonly int DefaultMaxPageSizeInBytes = 1024 * 1024;

        #endregion

        #region Public Properties

        public int MaxBlockSizeInBytes { get; }

        public int MaxPageSizeInBytes { get; }

        public bool Full
        {
            get
            {
                return _full || SizeInBytes >= MaxPageSizeInBytes;
            }
            set
            {
                _full = value;
            }
        }

        public int SizeInBytes
        {
            get
            {
                return _currentSize;
            }
        }

        #endregion

        #region Constructors

        public PageBuilderStatus() : this(DefaultMaxPageSizeInBytes, BlockBuilderStatus.DefaultMaxBlockSizeInBytes)
        {
        }

        public PageBuilderStatus(int maxPageSizeInBytes, int maxBlockSizeInBytes)
        {
            MaxPageSizeInBytes = maxPageSizeInBytes;
            MaxBlockSizeInBytes = maxBlockSizeInBytes;
        }

        #endregion

        #region Public Methods

        public BlockBuilderStatus CreateBlockBuilderStatus()
        {
            return new BlockBuilderStatus(this, MaxBlockSizeInBytes);
        }

        public void AddBytes(int bytes)
        {
            _currentSize += bytes;
        }

        public bool IsEmpty()
        {
            return _currentSize == 0;
        }

        public override string ToString()
        {
            return StringHelper.Build(this)
                .Add("maxSizeInBytes", MaxPageSizeInBytes)
                .Add("full", _full)
                .Add("currentSize", _currentSize)
                .ToString();
        }

        #endregion
    }
}
