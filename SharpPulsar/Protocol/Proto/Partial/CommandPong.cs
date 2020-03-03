using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPong
	{
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		
		public sealed class Builder
        {
            private CommandPong _pong;

            public Builder()
            {
                _pong = new CommandPong();
            }
			internal static Builder Create()
			{
				return new Builder();
			}

			
            public CommandPong Build()
            {
                return _pong;
            }
						
			
		}

		
	}

}
