using System;
using System.Collections.Generic;

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
	/// Base implementation for hash function providers.
	/// </summary>
	/// @param <P> base supported hash parameters type </param>
	public abstract class AbstractHashProvider<TP> : HashProvider where TP : HashParameters
	{

		private readonly Type _parametersClass;

		/// <summary>
		/// Constructs a new <seealso cref="AbstractHashProvider"/> with the given base
		/// parameters class.
		/// </summary>
		/// <param name="parametersClass"> the base hash parameters class supported </param>
        protected AbstractHashProvider(Type parametersClass)
		{
			_parametersClass = parametersClass;
		}

		public  ISet<HashSupport> QuerySupport(HashParameters @params)
		{
			if (!_parametersClass.IsInstanceOfType(@params))
			{
				return new HashSet<HashSupport>();
			}
			return QuerySupportTyped((TP)@params);
		}

		/// <summary>
		/// Implemented by subclasses to provide information about the available
		/// implementations corresponding to the given hash algorithm parameters.
		/// Called by <seealso cref="querySupport"/> if the hash parameters match the base
		/// type supported by this provider.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a set of flags indicating the level of support </returns>
		public abstract ISet<HashSupport> QuerySupportTyped(TP @params);

		/// <summary>
		/// Requests a hash function using the given parameters and support flags.
		/// This method is only responsible for checking support flags returned by
		/// <seealso cref="querySupportTyped"/>.
		/// <para>
		/// To support caching of stateless hash functions, call
		/// <seealso cref="getCacheable"/> from this method and implement
		/// <seealso cref="createCacheable"/>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <param name="required"> the required hash support flags </param>
		/// <returns> a hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		public abstract Hash Get(TP @params, ISet<HashSupport> required);

		/// <summary>
		/// Called by implementations that support caching of stateless hash
		/// functions when a cached instance is desired. If a cached instance is not
		/// available, this method calls <seealso cref="createCacheable"/> to create one,
		/// which is then cached (if caching is available).
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <param name="required"> the required hash support flags </param>
		/// <returns> a hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		public Hash GetCacheable(in TP @params, in ISet<HashSupport> required)
		{
            return CreateCacheable(@params, required);
		}

		
		/// <summary>
		/// Called by <seealso cref="getCacheable"/> to create new cacheable stateless hash
		/// functions. The default implementation simply throws
		/// <seealso cref="System.NotSupportedException"/>.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <param name="required"> the required hash support flags </param>
		/// <returns> a stateless hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		public virtual StatelessHash CreateCacheable(TP @params, ISet<HashSupport> required)
		{
			throw new NotSupportedException();
		}

		private Hash CastAndGet(HashParameters @params, ISet<HashSupport> required)
		{
			if (!_parametersClass.IsAssignableFrom(@params.GetType()))
			{
				throw new NotSupportedException();
			}
			return Get((TP)@params, required);
		}

		public StatefulHash CreateStateful(HashParameters @params)
        {
            Hash hash = CastAndGet(@params, new HashSet<HashSupport>(){ HashSupport.Stateful });
			if (hash is StatefulHash statefulHash)
			{
				return statefulHash;
			}
			if (hash is StatelessHash statelessHash)
			{
				return statelessHash.CreateStateful();
			}
			throw new NotSupportedException();
		}

		public StatelessIntHash GetStatelessInt(HashParameters @params)
		{
			Hash hash = CastAndGet(@params, new HashSet<HashSupport>{HashSupport.IntSized});
			if (hash is StatelessIntHash intHash)
			{
				return intHash;
			}
			if (hash is StatefulIntHash statefulIntHash)
			{
				return statefulIntHash.AsStateless();
			}
			throw new NotSupportedException();
		}

		public  StatelessLongHash GetStatelessLong(HashParameters @params)
		{
			Hash hash = CastAndGet(@params, new HashSet<HashSupport>{ HashSupport.LongSized});
			if (hash is StatelessLongHash longHash)
			{
				return longHash;
			}
			if (hash is StatefulLongHash statefulLongHash)
			{
				return statefulLongHash.AsStateless();
			}
			if (hash is StatelessIntHash intHash)
			{
				return new IntStatelessLongHash(intHash);
			}
			if (hash is StatefulIntHash statefulIntHash)
			{
				return new IntStatelessLongHash(statefulIntHash.AsStateless());
			}
			throw new NotSupportedException();
		}

		public IncrementalIntHash GetIncrementalInt(HashParameters @params)
		{
			Hash hash = CastAndGet(@params, new HashSet<HashSupport>{ HashSupport.IntSized, HashSupport.StatelessIncremental});
			if (hash is IncrementalIntHash intHash)
			{
				return intHash;
			}
			throw new NotSupportedException();
		}

		public IncrementalLongHash GetIncrementalLong(HashParameters @params)
		{
			Hash hash = CastAndGet(@params, new HashSet<HashSupport>{ HashSupport.LongSized, HashSupport.StatelessIncremental});
			if (hash is IncrementalLongHash)
			{
				return (IncrementalLongHash) hash;
			}
			throw new NotSupportedException();
		}
	}

}