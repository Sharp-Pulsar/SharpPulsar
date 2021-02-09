using System.Collections.Generic;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Common
{

	/// <summary>
	/// KeyShared policy for KeyShared subscription.
	/// </summary>
	public abstract class KeySharedPolicy
	{
		private KeySharedMode _keySharedMode;
        private bool _allowOutOfOrderDelivery = false;

		public static readonly int DefaultHashRangeSize = 2 << 15;

		public static KeySharedPolicyAutoSplit AutoSplitHashRange()
		{
			return new KeySharedPolicyAutoSplit();
		}

		public static KeySharedPolicySticky StickyHashRange()
		{
			return new KeySharedPolicySticky();
		}

		public abstract void Validate();
        public virtual bool AllowOutOfOrderDelivery => _allowOutOfOrderDelivery;

        /// <summary>
        /// If enabled, it will relax the ordering requirement, allowing the broker to send out-of-order messages in case of
        /// failures. This will make it faster for new consumers to join without being stalled by an existing slow consumer.
        /// 
        /// <para>In this case, a single consumer will still receive all the keys, but they may be coming in different orders.
        /// 
        /// </para>
        /// </summary>
        /// <param name="allowOutOfOrderDelivery">
        ///            whether to allow for out of order delivery </param>
        /// <returns> KeySharedPolicy instance </returns>
        public virtual KeySharedPolicy SetAllowOutOfOrderDelivery(bool allowOutOfOrderDelivery)
        {
            _allowOutOfOrderDelivery = allowOutOfOrderDelivery;
            return this;
        }
		public virtual KeySharedMode? KeySharedMode => _keySharedMode;

        public virtual int HashRangeTotal => DefaultHashRangeSize;

        /// <summary>
		/// Sticky attach topic with fixed hash range.
		/// 
		/// <para>Total hash range Size is 65536, using the sticky hash range policy should ensure that the provided ranges by
		/// all consumers can cover the total hash range [0, 65535]. If not, while broker dispatcher can't find the consumer
		/// for message, the cursor will rewind.
		/// </para>
		/// </summary>
		public class KeySharedPolicySticky : KeySharedPolicy
		{
			private readonly IList<Range> _ranges;

			public KeySharedPolicySticky()
			{
				_keySharedMode = Common.KeySharedMode.Sticky;
				_ranges = new List<Range>();
			}

			public virtual KeySharedPolicySticky GetRanges(params Range[] ranges)
			{
				((List<Range>)_ranges).AddRange(new List<Range>(ranges));
				return this;
			}

			public override void Validate()
			{
				if (_ranges.Count == 0)
				{
					throw new System.ArgumentException("Ranges for KeyShared policy must not be empty.");
				}
				for (int i = 0; i < _ranges.Count; i++)
				{
					Range range1 = _ranges[i];
					if (range1.Start < 0 || range1.End > DefaultHashRangeSize)
					{
						throw new System.ArgumentException("Ranges must be [0, 65535] but provided range is " + range1);
					}
					for (int j = 0; j < _ranges.Count; j++)
					{
						Range range2 = _ranges[j];
						if (i != j && range1.Intersect(range2) != null)
						{
							throw new System.ArgumentException("Ranges for KeyShared policy with overlap between " + range1 + " and " + range2);
						}
					}
				}
			}

			public virtual IList<Range> Ranges => _ranges;
        }

		/// <summary>
		/// Auto split hash range key shared policy.
		/// </summary>
		public class KeySharedPolicyAutoSplit : KeySharedPolicy
		{

			public KeySharedPolicyAutoSplit()
			{
				_keySharedMode = Common.KeySharedMode.AutoSplit;
			}

			public override void Validate()
			{
				// do nothing here
			}
		}
	}

}