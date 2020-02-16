using System.Runtime.InteropServices;

/// <summary>
///*****************************************************************************
/// Copyright 2014 Trevor Robinson
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// *****************************************************************************
/// </summary>
namespace SharpPulsar.Protocol.Circe
{

    /// <summary>
	/// Implementation of CRC-32C using the SSE 4.2 CRC instruction.
	/// </summary>
	public sealed class Sse42Crc32C : AbstractIncrementalIntHash, IncrementalIntHash
	{

		private static readonly bool SUPPORTED = CheckSupported();

		private static bool CheckSupported()
		{
			try
			{
				loadLibraryFromJar("/lib/libcirce-checksum." + libType());
				return nativeSupported();
			}
			catch (Exception w)
			{
				return false;
			}
		}

		/// <summary>
		/// Returns whether SSE 4.2 CRC-32C is supported on this system.
		/// </summary>
		/// <returns> true if this class is supported, false if not </returns>
		public static bool Supported
		{
			get
			{
				return SUPPORTED;
			}
		}

		private readonly long config;

		public Sse42Crc32C()
		{
			config = 0;
		}

		public Sse42Crc32C(int[] chunkWords)
		{
			if (chunkWords.Length == 0)
			{
				config = 0;
			}
			else
			{
				config = allocConfig(chunkWords);
				if (config == 0)
				{
					throw new Exception("CRC32C configuration allocation failed");
				}
			}
		}

		public  void Finalize()
		{
			if (config != 0)
			{
				freeConfig(config);
			}
		}

		public override string Algorithm()
		{
			return Circe.CrcParameters.Crc32C.algorithm();
		}

		public override int Length()
		{
			return 4;
		}

		public override bool SupportsUnsafe()
		{
			return true;
		}

		public override int Calculate(long Address, long Length)
		{
			return nativeUnsafe(Initial(), Address, Length, config);
		}

		public override int Resume(int Current, ByteBuffer Input)
		{
			if (Input.Direct)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int result = nativeDirectBuffer(current, input, input.position(), input.remaining(), config);
				int Result = nativeDirectBuffer(Current, Input, Input.position(), Input.remaining(), config);
				Input.position(Input.limit());
				return Result;
			}

			return base.Resume(Current, Input);
		}

		public override int Resume(int Current, long Address, long Length)
		{
			return nativeUnsafe(Current, Address, Length, config);
		}

		public override int Initial()
		{
			return 0;
		}

		public override int ResumeUnchecked(int Current, sbyte[] Input, int Index, int Length)
		{
			return nativeArray(Current, Input, Index, Length, config);
		}

		[DllImport("unknown")]
		private static extern boolean NativeSupported();

		[DllImport("unknown")]
		private static extern int NativeArray(int Current, sbyte[] Input, int Index, int Length, long Config);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		private static extern int NativeDirectBuffer(int Current, ByteBuffer Input, int Offset, int Length, long Config);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		private static extern int NativeUnsafe(int Current, long Address, long Length, long Config);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		private static extern long AllocConfig(int[] ChunkWords);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		private static extern void FreeConfig(long Config);
	}

}