using System;
using System.Collections.Generic;

namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.type.AbstractType.java
    /// </summary>
    public abstract class AbstractType : IType
    {
        #region Protected Fields

        protected readonly TypeSignature Signature;

        protected readonly System.Type JavaType;

        #endregion

        #region Constructors

        public AbstractType(TypeSignature signature, System.Type type)
        {
            this.Signature = signature;
            this.JavaType = type;
        }

        #endregion

        #region Public Methods

        public virtual string GetDisplayName()
        {
            return this.Signature.ToString();
        }

        public virtual System.Type GetJavaType()
        {
            return this.JavaType;
        }

        public virtual IEnumerable<IType> GetTypeParameter()
        {
            return new IType[0];
        }

        public virtual TypeSignature GetTypeSignature()
        {
            return this.Signature;
        }

        public virtual bool IsComparable()
        {
            return false;
        }

        public virtual bool IsOrderable()
        {
            return false;
        }

        public override string ToString()
        {
            return this.Signature.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Signature.Equals(((IType)obj).GetTypeSignature());
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(this.Signature);
        }

        public virtual long Hash(IBlock block, int position)
        {
            throw new InvalidOperationException($"{this.GetTypeSignature()} type is not comparable.");
        }

        public virtual int CompareTo(IBlock leftBlock, int leftPosition, IBlock rightBlock, int rightPosition)
        {
            throw new InvalidOperationException($"{this.GetTypeSignature()} type is not orderable.");
        }

        public virtual bool EqualTo(IBlock leftBlock, int leftPosition, IBlock rightBlock, int rightPosition)
        {
            throw new InvalidOperationException($"{this.GetTypeSignature()} type is not comparable.");
        }

        public virtual bool GetBoolean(IBlock block, int position)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteBoolean(IBlockBuilder blockBuilder, bool value)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual long GetLong(IBlock block, int position)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteLong(IBlockBuilder blockBuilder, long value)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual double GetDouble(IBlock block, int position)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteDouble(IBlockBuilder blockBuilder, double value)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual Slice GetSlice(IBlock block, int position)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteSlice(IBlockBuilder blockBuilder, Slice value)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteSlice(IBlockBuilder blockBuilder, Slice value, int offset, int length)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual object GetObject(IBlock block, int position)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual void WriteObject(IBlockBuilder blockBuilder, object value)
        {
            throw new InvalidOperationException(this.GetType().Name);
        }

        public virtual IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries)
        {
            return this.CreateBlockBuilder(blockBuilderStatus, expectedEntries, 0);
        }

        #endregion

        #region Not Implemented Interface Methods

        public virtual void AppendTo(IBlock block, int position, IBlockBuilder blockBuilder)
        {
            throw new NotImplementedException();
        }

        public virtual IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries, int expectedBytesPerEntry)
        {
            throw new NotImplementedException();
        }

        public virtual object GetObjectValue(IConnectorSession session, IBlock block, int position)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
