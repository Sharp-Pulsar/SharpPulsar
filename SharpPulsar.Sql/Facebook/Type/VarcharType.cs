using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.type.VarcharType.java
    /// </summary>
    public sealed class VarcharType : AbstractVariableWidthType
    {
        #region Private Fields

        private int _length;

        #endregion

        #region Public Properties

        public static readonly int UnboundedLength = Int32.MaxValue;
        public static readonly int MaxLength = Int32.MaxValue - 1;
        public static readonly VarcharType Varchar = new VarcharType(UnboundedLength);

        public int Length
        {
            get
            {
                if (this.IsUnbounded())
                {
                    throw new InvalidOperationException("Cannot get size of unbounded VARCHAR.");
                }

                return this._length;
            }
        }

        #endregion

        #region Constructors

        private VarcharType(int length) : base(new TypeSignature(StandardTypes.Varchar, new TypeSignatureParameter((long)length)), typeof(string))
        {
            ParameterCondition.OutOfRange(length >= 0, "length");

            this._length = length;
        }

        #endregion

        #region Public Methods

        public bool IsUnbounded()
        {
            return this._length == UnboundedLength;
        }

        public override bool IsComparable()
        {
            return true;
        }

        public override bool IsOrderable()
        {
            return true;
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

            VarcharType other = (VarcharType)obj;

            return Object.Equals(this._length, other._length);
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(this._length);
        }

        public string DisplayName()
        {
            if (this._length == UnboundedLength)
            {
                return this.Signature.Base;
            }

            return this.Signature.ToString();
        }

        public override string ToString()
        {
            return DisplayName();
        }

        #endregion
    }
}
