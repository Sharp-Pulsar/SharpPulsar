﻿using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
	/// <summary>
	/// Pulsar Schema compatibility strategy.
	/// </summary>
	public sealed class SchemaCompatibilityStrategy
	{

		/// <summary>
		/// Undefined.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy UNDEFINED = new SchemaCompatibilityStrategy("UNDEFINED", InnerEnum.UNDEFINED);

		/// <summary>
		/// Always incompatible.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy AlwaysIncompatible = new SchemaCompatibilityStrategy("AlwaysIncompatible", InnerEnum.AlwaysIncompatible);

		/// <summary>
		/// Always compatible.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy AlwaysCompatible = new SchemaCompatibilityStrategy("AlwaysCompatible", InnerEnum.AlwaysCompatible);

		/// <summary>
		/// Messages written by an old schema can be read by a new schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy BACKWARD = new SchemaCompatibilityStrategy("BACKWARD", InnerEnum.BACKWARD);

		/// <summary>
		/// Messages written by a new schema can be read by an old schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy FORWARD = new SchemaCompatibilityStrategy("FORWARD", InnerEnum.FORWARD);

		/// <summary>
		/// Equivalent to both FORWARD and BACKWARD.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy FULL = new SchemaCompatibilityStrategy("FULL", InnerEnum.FULL);

		/// <summary>
		/// Be similar to BACKWARD, BACKWARD_TRANSITIVE ensure all previous version schema can
		/// be read by the new schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy BackwardTransitive = new SchemaCompatibilityStrategy("BackwardTransitive", InnerEnum.BackwardTransitive);

		/// <summary>
		/// Be similar to FORWARD, FORWARD_TRANSITIVE ensure new schema can be ready by all previous
		/// version schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy ForwardTransitive = new SchemaCompatibilityStrategy("ForwardTransitive", InnerEnum.ForwardTransitive);

		/// <summary>
		/// Equivalent to both FORWARD_TRANSITIVE and BACKWARD_TRANSITIVE.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy FullTransitive = new SchemaCompatibilityStrategy("FullTransitive", InnerEnum.FullTransitive);

		private static readonly List<SchemaCompatibilityStrategy> valueList = new List<SchemaCompatibilityStrategy>();

		static SchemaCompatibilityStrategy()
		{
			valueList.Add(UNDEFINED);
			valueList.Add(AlwaysIncompatible);
			valueList.Add(AlwaysCompatible);
			valueList.Add(BACKWARD);
			valueList.Add(FORWARD);
			valueList.Add(FULL);
			valueList.Add(BackwardTransitive);
			valueList.Add(ForwardTransitive);
			valueList.Add(FullTransitive);
		}

		public enum InnerEnum
		{
			UNDEFINED,
			AlwaysIncompatible,
			AlwaysCompatible,
			BACKWARD,
			FORWARD,
			FULL,
			BackwardTransitive,
			ForwardTransitive,
			FullTransitive
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private SchemaCompatibilityStrategy(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}


		public static bool isUndefined(SchemaCompatibilityStrategy Strategy)
		{
			return Strategy == null || Strategy == SchemaCompatibilityStrategy.UNDEFINED;
		}

		public static SchemaCompatibilityStrategy fromAutoUpdatePolicy(SchemaAutoUpdateCompatibilityStrategy Strategy)
		{
			if (Strategy == null)
			{
				return null;
			}
			switch (Strategy)
			{
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.Backward:
					return BACKWARD;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.Forward:
					return FORWARD;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.Full:
					return FULL;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.AlwaysCompatible:
					return AlwaysCompatible;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.ForwardTransitive:
					return ForwardTransitive;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.BackwardTransitive:
					return BackwardTransitive;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.FullTransitive:
					return FullTransitive;
				case SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy.AutoUpdateDisabled:
				default:
					return AlwaysIncompatible;
			}
		}

		public static SchemaCompatibilityStrategy[] values()
		{
			return valueList.ToArray();
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static SchemaCompatibilityStrategy valueOf(string name)
		{
			foreach (SchemaCompatibilityStrategy enumInstance in SchemaCompatibilityStrategy.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}