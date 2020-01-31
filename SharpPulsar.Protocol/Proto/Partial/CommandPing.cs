using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPing : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandPing.newBuilder() to construct.
		internal static ThreadLocalPool<CommandPing> _pool = new ThreadLocalPool<CommandPing>(handle => new CommandPing(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandPing(ThreadLocalPool.Handle handle)
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

		public CommandPing(bool NoInit)
		{
		}

		internal static readonly CommandPing _defaultInstance;
		public static CommandPing DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandPing DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

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
		public static Builder NewBuilder(CommandPing Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandPing.newBuilder()
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

			public CommandPing DefaultInstanceForType
			{
				get
				{
					return CommandPing.DefaultInstance;
				}
			}

			public CommandPing Build()
			{
				CommandPing Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandPing BuildParsed()
			{
				CommandPing Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandPing BuildPartial()
			{
				CommandPing Result = CommandPing._pool.Take();
				return Result;
			}

			public Builder MergeFrom(CommandPing Other)
			{
				if (Other == CommandPing.DefaultInstance)
				{
					return this;
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					return true;
				}
			}
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


			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandPing)
		}

		static CommandPing()
		{
			_defaultInstance = new CommandPing(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandPing)
	}

}
