/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Sql.Facebook.Type
{
    using Block = io.prestosql.spi.block.Block;
	using BlockBuilder = io.prestosql.spi.block.BlockBuilder;
	using ConnectorSession = io.prestosql.spi.connector.ConnectorSession;

	public sealed class VarcharType : AbstractVariableWidthType
	{
		public static readonly int UnboundedLength = int.MaxValue;
		public static readonly int MaxLength = int.MaxValue - 1;
		public static readonly VarcharType Varchar = new VarcharType(UnboundedLength);

		public static VarcharType CreateUnboundedVarcharType()
		{
			return Varchar;
		}

		public static VarcharType CreateVarcharType(int length)
		{
			if (length > MaxLength || length < 0)
			{
				// Use createUnboundedVarcharType for unbounded VARCHAR.
				throw new System.ArgumentException("Invalid VARCHAR length " + length);
			}
			return new VarcharType(length);
		}

		public static TypeSignature GetParametrizedVarcharSignature(string param)
		{
			return new TypeSignature(StandardTypes.Varchar, TypeSignatureParameter.Of(param));
		}

		private readonly int _length;

		private VarcharType(int length) : base(new TypeSignature(StandardTypes.Varchar, TypeSignatureParameter.Of((long) length)), typeof(Slice))
		{

			if (length < 0)
			{
				throw new System.ArgumentException("Invalid VARCHAR length " + length);
			}
			this._length = length;
		}

		public int? Length
		{
			get
			{
				if (Unbounded)
				{
					return null;
				}
				return _length;
			}
		}

		public int BoundedLength
		{
			get
			{
				if (Unbounded)
				{
					throw new System.InvalidOperationException("Cannot get size of unbounded VARCHAR.");
				}
				return _length;
			}
		}

		public bool Unbounded => _length == UnboundedLength;

        public bool Comparable => true;

        public bool Orderable => true;

        public override object GetObjectValue(ConnectorSession session, Block block, int position)
		{
			if (block.isNull(position))
			{
				return null;
			}

			return block.getSlice(position, 0, block.getSliceLength(position)).toStringUtf8();
		}

		public override bool EqualTo(Block leftBlock, int leftPosition, Block rightBlock, int rightPosition)
		{
			int leftLength = leftBlock.getSliceLength(leftPosition);
			int rightLength = rightBlock.getSliceLength(rightPosition);
			if (leftLength != rightLength)
			{
				return false;
			}
			return leftBlock.equals(leftPosition, 0, rightBlock, rightPosition, 0, leftLength);
		}

		public override long Hash(Block block, int position)
		{
			return block.hash(position, 0, block.getSliceLength(position));
		}

		public override int CompareTo(Block leftBlock, int leftPosition, Block rightBlock, int rightPosition)
		{
			int leftLength = leftBlock.getSliceLength(leftPosition);
			int rightLength = rightBlock.getSliceLength(rightPosition);
			return leftBlock.compareTo(leftPosition, 0, leftLength, rightBlock, rightPosition, 0, rightLength);
		}

		public Range Range
		{
			get
			{
				if (_length > 100)
				{
					// The max/min values may be materialized in the plan, so we don't want them to be too large.
					// Range comparison against large values are usually nonsensical, too, so no need to support them
					// beyond a certain size. They specific choice above is arbitrary and can be adjusted if needed.
					return null;
				}
    
				int codePointSize = Slice.LengthOfCodePoint(MAX_CODE_POINT);
    
				Slice max = Slices.allocate(codePointSize * _length);
				int position = 0;
				for (int i = 0; i < _length; i++)
				{
					position += SliceUtf8.setCodePointAt(MAX_CODE_POINT, max, position);
				}
    
				return (new Range(Slices.EMPTY_SLICE, max));
			}
		}

		public override void AppendTo(Block block, int position, BlockBuilder blockBuilder)
		{
			if (block.isNull(position))
			{
				blockBuilder.appendNull();
			}
			else
			{
				block.writeBytesTo(position, 0, block.getSliceLength(position), blockBuilder);
				blockBuilder.closeEntry();
			}
		}

		public override Slice GetSlice(Block block, int position)
		{
			return block.getSlice(position, 0, block.getSliceLength(position));
		}

		public void WriteString(BlockBuilder blockBuilder, string value)
		{
			WriteSlice(blockBuilder, Slices.utf8Slice(value));
		}

		public override void WriteSlice(BlockBuilder blockBuilder, Slice value)
		{
			WriteSlice(blockBuilder, value, 0, value.length());
		}

		public override void WriteSlice(BlockBuilder blockBuilder, Slice value, int offset, int length)
		{
			blockBuilder.writeBytes(value, offset, length).closeEntry();
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}
			if (o == null || this.GetType() != o.GetType())
			{
				return false;
			}

			VarcharType other = (VarcharType) o;

			return Objects.equals(this._length, other._length);
		}

		public override int GetHashCode()
		{
			return Objects.hash(_length);
		}

		public override string DisplayName
		{
			get
			{
				if (_length == UnboundedLength)
				{
					return BaseName;
				}
    
				return TypeSignature.ToString();
			}
		}

		public override string ToString()
		{
			return DisplayName;
		}
	}

}