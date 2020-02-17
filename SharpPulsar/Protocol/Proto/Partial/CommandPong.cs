using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPong : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandPong.newBuilder() to construct.
		internal static ThreadLocalPool<CommandPong> _pool = new ThreadLocalPool<CommandPong>(handle => new CommandPong(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandPong(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this.MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public CommandPong(bool NoInit)
		{
		}

		internal static readonly CommandPong _defaultInstance;
		public static CommandPong DefaultInstance => _defaultInstance;

        public CommandPong DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
		}
		internal sbyte MemoizedIsInitialized = -1;
		public bool Initialized
		{
			get
			{
				sbyte IsInitialized = MemoizedIsInitialized;
				if (IsInitialized != -1)
				{
					return IsInitialized == 1;
				}

				MemoizedIsInitialized = 1;
				return true;
			}
		}

		public void WriteTo(ByteBufCodedOutputStream Output)
		{
			var _ = SerializedSize;
		}

		internal int MemoizedSerializedSize = -1;
		public int SerializedSize => CalculateSize();

		internal const long SerialVersionUID = 0L;
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(CommandPong Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandPong.newBuilder()
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
				MaybeForceBuilderInitialization();
			}
			public void Recycle()
			{
				Clear();
				if (_handle != null)
				{
					_handle.Release(this);
				}
			}

			public void MaybeForceBuilderInitialization()
			{
			}
			internal static Builder Create()
			{
				return _pool.Take();
			}

			public Builder Clear()
			{
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandPong DefaultInstanceForType => CommandPong.DefaultInstance;

            public CommandPong Build()
			{
				CommandPong Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}
						
			public CommandPong BuildParsed()
			{
				CommandPong Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandPong BuildPartial()
			{
				CommandPong Result = CommandPong._pool.Take();
				return Result;
			}

			public Builder MergeFrom(CommandPong Other)
			{
				if (Other == CommandPong.DefaultInstance)
				{
					return this;
				}
				return this;
			}

			public bool Initialized => true;

            public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry extensionRegistry)
			{
				while (true)
				{
					int Tag = input.ReadTag();
					switch (Tag)
					{
						case 0:

							return this;
						default:
							{
								if (!input.SkipField(Tag))
								{

									return this;
								}
								break;
							}
					}
				}
			}


			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandPong)
		}

		static CommandPong()
		{
			_defaultInstance = new CommandPong(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandPong)
	}

}
