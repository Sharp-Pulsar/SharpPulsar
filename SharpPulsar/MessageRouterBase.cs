using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar
{
	[Serializable]
	public abstract class MessageRouterBase : IMessageRouter
	{
		public abstract int ChoosePartition<T>(IMessage<T> msg, TopicMetadata metadata);
		public abstract int ChoosePartition<T>(IMessage<T> msg);
		private const long SerialVersionUID = 1L;

		protected internal readonly IHash Hash;

		internal MessageRouterBase(HashingScheme hashingScheme)
		{
			switch (hashingScheme)
			{
				case HashingScheme.JavaStringHash:
					Hash = JavaStringHash.Instance;
					break;
				case HashingScheme.Murmur332Hash:
				default:
					Hash = Murmur332Hash.Instance;
					break;
			}
		}
	}
}
