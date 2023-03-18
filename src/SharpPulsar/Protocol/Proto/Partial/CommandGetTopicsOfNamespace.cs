using System;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetTopicsOfNamespace
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {

            private readonly CommandGetTopicsOfNamespace _command;

            public Builder()
            {
                _command = new CommandGetTopicsOfNamespace();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandGetTopicsOfNamespace Build()
            {
                return _command;
            }

			
            public Builder SetRequestId(long value)
            {
                _command.RequestId = (ulong)value;
				return this;
			}
			
			public string GetNamespace()
            {
                return _command.Namespace;
            }
			public Builder SetNamespace(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}

                _command.Namespace = value;
				return this;
			}
			
            public Builder SetMode(Mode value)
			{
                _command.mode = value;
				return this;
			}
			
		}

	}

}
