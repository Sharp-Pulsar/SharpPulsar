using System.Collections.Generic;
using System.Linq;

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
namespace SharpPulsar.Util.Circe
{

	/// <summary>
	/// Static utility methods for discovering <seealso cref="HashProvider"/> instances.
	/// </summary>
	public sealed class HashProviders
	{

		internal static readonly ICollection<HashProvider> AllProvidersConflict = AllProviders;

		private static ICollection<HashProvider> AllProviders
		{
			get
			{
				ServiceLoader<HashProvider> loader = ServiceLoader.load(typeof(HashProvider));
				LinkedList<HashProvider> providers = new LinkedList<HashProvider>();
				foreach (HashProvider provider in loader)
				{
					providers.AddLast(provider);
				}
				return new List<HashProvider>(providers);
			}
		}

		private HashProviders()
		{
		}

		/// <summary>
		/// Returns an iterator over all known <seealso cref="HashProvider"/> instances.
		/// </summary>
		/// <returns> an iterator over all HashProviders </returns>
		public static IEnumerator<HashProvider> Iterator()
		{
			return AllProvidersConflict.GetEnumerator();
		}

		/// <summary>
		/// Returns the best hash provider supporting at least a stateful
		/// implementation of a hash function with the given parameters.
		/// </summary>
		/// <param name="params"> the parameters defining the hash function </param>
		/// <returns> the best hash provider for the given parameters </returns>
		/// <exception cref="UnsupportedOperationException"> if no provider supports the
		///             parameters </exception>
		public static HashProvider Best(HashParameters @params)
		{
			return Best(@params, new HashSet<HashSupport>(){ HashSupport.Stateful});
		}

		/// <summary>
		/// Returns the best hash provider supporting at least the given flags for a
		/// hash function with the given parameters.
		/// </summary>
		/// <param name="params"> the parameters defining the hash function </param>
		/// <param name="required"> the required support flags for a provider to be
		///            considered </param>
		/// <returns> the best hash provider for the given parameters </returns>
		/// <exception cref="UnsupportedOperationException"> if no provider supports the
		///             parameters </exception>
		public static HashProvider Best(HashParameters @params, ISet<HashSupport> required)
		{
			HashProvider result = null;
			ISet<HashSupport> resultSupport = null;
			foreach (HashProvider provider in AllProvidersConflict)
			{
				ISet<HashSupport> support = provider.QuerySupport(@params);
				if (!support.Except(required).Any() && (result == null || HashSupport.Compare(support, resultSupport) < 0))
				{
					result = provider;
					resultSupport = support;
				}
			}
			if (result == null)
			{
				throw new System.NotSupportedException();
			}
			return result;
		}

		/// <summary>
		/// Returns a map of hash providers supporting at least a stateful
		/// implementation of a hash function with the given parameters.
		/// </summary>
		/// <param name="params"> the parameters defining the hash function </param>
		/// <returns> a sorted map of hash support flags to hash providers </returns>
		public static SortedDictionary<ISet<HashSupport>, HashProvider> Search(HashParameters @params)
		{
			return Search(@params, new HashSet<HashSupport>(){ HashSupport.Stateful});
		}

		/// <summary>
		/// Returns a map of hash providers supporting at least the given flags for a
		/// hash function with the given parameters.
		/// </summary>
		/// <param name="params"> the parameters defining the hash function </param>
		/// <param name="required"> the required support flags for a provider to be included </param>
		/// <returns> a sorted map of hash support flags to hash providers </returns>
		public static SortedDictionary<ISet<HashSupport>, HashProvider> Search(HashParameters @params, ISet<HashSupport> required)
		{
			SortedDictionary<ISet<HashSupport>, HashProvider> result = new SortedDictionary<ISet<HashSupport>, HashProvider>(new HashSupport.SetComparator());
			foreach (HashProvider provider in AllProvidersConflict)
			{
				var support = provider.QuerySupport(@params);
				if (!support.Except(required).Any())
				{
					result[support] = provider;
				}
			}
			return result;
		}
	}

}