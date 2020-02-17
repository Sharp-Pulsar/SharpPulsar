﻿/// <summary>
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


	public class IncrementalIntStatefulHash : AbstractStatefulHash, StatefulIntHash
	{

		internal readonly AbstractIncrementalIntHash Stateless;
		internal int Current;

		public IncrementalIntStatefulHash(AbstractIncrementalIntHash stateless)
		{
			Stateless = stateless;
		}

		public StatelessIntHash AsStateless()
		{
			return Stateless;
		}

		public override string Algorithm()
		{
			return Stateless.Algorithm();
		}

		public override int Length()
		{
			return Stateless.Length();
		}

		public new bool SupportsUnsafe()
		{
			return Stateless.SupportsUnsafe();
		}

		public override StatefulHash CreateNew()
		{
			return new IncrementalIntStatefulHash(Stateless);
		}

		public override bool SupportsIncremental()
		{
			return true;
		}

		public override void Reset()
		{
			Current = Stateless.Initial();
		}

		public new void Update(ByteBuffer input)
		{
			Current = Stateless.Resume(Current, input);
		}

		public new void Update(long address, long length)
		{
			Current = Stateless.Resume(Current, address, length);
		}

		public override void UpdateUnchecked(sbyte[] input, int index, int length)
		{
			Current = Stateless.ResumeUnchecked(Current, input, index, length);
		}

		public override int Int => Current;

        public override long Long => Current;
    }

}