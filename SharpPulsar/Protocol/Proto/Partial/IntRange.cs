using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;

namespace SharpPulsar.Protocol.Proto
{
	public partial class IntRange
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private IntRange _int;

            public Builder()
            {
                _int = new IntRange();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public IntRange Build()
            {
                return _int;
            }

            public Builder SetStart(int value)
            {
                _int.Start = value;
				return this;
			}
			
            public Builder SetEnd(int value)
            {
                _int.End = value;
				return this;
			}
			
		}

	}

}
