
using System;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Proto
{
	public partial class KeySharedMeta
	{
		
        public IntRange GetHashRanges(int index)
		{
			return hashRanges[index];
		}
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly KeySharedMeta _meta;

            public Builder()
            {
                _meta = new KeySharedMeta();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public KeySharedMeta Build()
            {
                return _meta;
            }

            public Builder SetKeySharedMode(KeySharedMode value)
            {
                _meta.keySharedMode = value;
				return this;
			}
			
            public IntRange GetHashRanges(int index)
			{
				return _meta.hashRanges[index];
			}
			public Builder SetHashRanges(int index,IntRange value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _meta.hashRanges[index] = value;

				return this;
			}
			public Builder SetHashRanges(int index, IntRange.Builder builderForValue)
			{
                _meta.hashRanges[index] = builderForValue.Build();

				return this;
			}
			public Builder AddHashRanges(IntRange value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _meta.hashRanges.Add(value);

				return this;
			}
			public Builder AddHashRanges(int index,IntRange value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _meta.hashRanges.Insert(index, value);

				return this;
			}
			public Builder AddHashRanges(IntRange.Builder builderForValue)
			{
                _meta.hashRanges.Add(builderForValue.Build());

				return this;
			}
			public Builder AddHashRanges(int index,IntRange.Builder builderForValue)
			{
                _meta.hashRanges.Insert(index, builderForValue.Build());

				return this;
			}
			public Builder AddAllHashRanges(IEnumerable<IntRange> values)
			{
                _meta.hashRanges.AddRange(values);

				return this;
			}
			
		}

	}

}
