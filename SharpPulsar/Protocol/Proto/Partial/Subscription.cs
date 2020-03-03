using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class Subscription
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private Subscription _sub;

            public Builder()
            {
                _sub = new Subscription();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

            public Subscription Build()
            {
                return _sub;
            }
			
			public string GetTopic()
            {
                return _sub.Topic;
            }
			public Builder SetTopic(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _sub.Topic = value;

				return this;
			}
			
			public string GetSubscription()
            {
                return _sub.subscription;
            }
			public Builder GetSubscription(string value)
			{
				if (string.ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _sub.subscription = value;

				return this;
			}
			
		}


	}

}
