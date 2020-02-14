using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.BlockBuilderStatus.java
    /// </summary>
    public class BlockBuilderStatus
    {
        #region Private Fields

        private PageBuilderStatus _pageBuilderStatus;

        private int _currentSize;

        #endregion

        #region Public Properties

        public int MaxBlockSizeInBytes { get; }

        #endregion

        #region Public Fields

        public static readonly int InstanceSize = DeepInstanceSize(typeof(BlockBuilderStatus));

        public static readonly int DefaultMaxBlockSizeInBytes = 64 * 1024;

        #endregion

        #region Constructors

        public BlockBuilderStatus() : this(new PageBuilderStatus(PageBuilderStatus.DefaultMaxPageSizeInBytes, DefaultMaxBlockSizeInBytes), DefaultMaxBlockSizeInBytes)
        {
        }

        public BlockBuilderStatus(PageBuilderStatus pageBuilderStatus, int maxBlockSizeInBytes)
        {
            _pageBuilderStatus = pageBuilderStatus ?? throw new ArgumentNullException("pageBuilderStatus");
            MaxBlockSizeInBytes = maxBlockSizeInBytes;
        }

        #endregion

        #region Private Methods

        private static int DeepInstanceSize(System.Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsArray)
            {
                throw new ArgumentException($"Cannot determine size of {type.Name} because it contains an array.");
            }

            if (type.IsInterface)
            {
                throw new ArgumentException($"{type.Name} is an interface.");
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException($"{type.Name} is abstract.");
            }

            if (!type.BaseType.Equals(typeof(object)))
            {
                throw new ArgumentException($"Cannot determine size of a subclass. {type.Name} extends from {type.BaseType.Name}.");
            }

            int size = Marshal.SizeOf(type);

            foreach (PropertyInfo info in type.GetProperties())
            {
                if (!info.PropertyType.IsPrimitive)
                {
                    size += DeepInstanceSize(info.PropertyType);
                }
            }

            return size;
        }

        #endregion

        #region Public Methods

        public void AddBytes(int bytes)
        {
            _currentSize += bytes;
            _pageBuilderStatus.AddBytes(bytes);

            if (_currentSize >= MaxBlockSizeInBytes)
            {
                _pageBuilderStatus.Full = true;
            }
        }

        public override string ToString()
        {
            return StringHelper.Build(this)
                .Add("maxSizeInBytes", MaxBlockSizeInBytes)
                .Add("currentSize", _currentSize)
                .ToString();
        }

        #endregion
    }
}
