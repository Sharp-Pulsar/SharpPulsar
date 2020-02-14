using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.spi.type.AbstractVariableWidthType.java
    /// </summary>
    public abstract class AbstractVariableWidthType : AbstractType, IVariableWidthType
    {
        #region Private Fields

        private static readonly int ExpectedBytesPerEntry = 32;

        #endregion

        #region Constructors

        protected AbstractVariableWidthType(TypeSignature signature, System.Type type) : base(signature, type)
        {
        }

        #endregion

        #region Public Methods

        public override IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries, int expectedBytesPerEntry)
        {
            int maxBlockSizeInBytes;

            if (blockBuilderStatus == null)
            {
                maxBlockSizeInBytes = BlockBuilderStatus.DefaultMaxBlockSizeInBytes;
            }
            else
            {
                maxBlockSizeInBytes = blockBuilderStatus.MaxBlockSizeInBytes;
            }

            var expectedBytes = Math.Min(expectedEntries * expectedBytesPerEntry, maxBlockSizeInBytes);

            return new VariableWidthBlockBuilder(
                blockBuilderStatus,
                (expectedBytesPerEntry == 0 ? expectedEntries : Math.Min(expectedEntries, maxBlockSizeInBytes / expectedBytesPerEntry)),
                expectedBytes
            );
        }

        public override IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries)
        {
            return this.CreateBlockBuilder(blockBuilderStatus, expectedEntries, ExpectedBytesPerEntry);
        }

        #endregion
    }
}
