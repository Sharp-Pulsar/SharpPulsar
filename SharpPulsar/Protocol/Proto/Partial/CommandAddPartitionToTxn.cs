using System;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAddPartitionToTxn
	{
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private readonly CommandAddPartitionToTxn _partition;

            public Builder()
            {
                    _partition = new CommandAddPartitionToTxn();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandAddPartitionToTxn Build()
            {
                return _partition;
            }

			
            public Builder SetRequestId(long value)
            {
                _partition.RequestId = (ulong)value;
				return this;
			}
			
            public Builder SetTxnidLeastBits(long value)
            {
                _partition.TxnidLeastBits = (ulong)value;
				return this;
			}
			
            public Builder SetTxnidMostBits(long value)
            {
                _partition.TxnidMostBits = (ulong) value;
				return this;
			}
			
            public string GetPartitions(int index)
			{
				return _partition.Partitions[index];
			}
			public Builder SetPartitions(int index, string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
				_partition.Partitions.Insert(index, value);

				return this;
			}
			public Builder AddPartitions(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
				_partition.Partitions.Add(value);

				return this;
			}
			public Builder AddAllPartitions(IEnumerable<string> values)
			{
				_partition.Partitions.AddRange(values);

				return this;
			}
			
		}

	}

}
